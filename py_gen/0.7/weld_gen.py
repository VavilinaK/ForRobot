__version__ = '0.7.0'
# 0.1.0 - начальная версия простые прямоугольные детали
# 0.2.0 - добавлены детали с наклоном, трапециевидные детали, изменение параметров сварки в зависимости от скорости
# 0.3.0 - добавлены детали с разными расстояниями между рёбрами
# 0.3.1 - рефакторинг, комментарии
# 0.4.0 - рёбра под углом и с индивидуальными отступами, смещение 0 детали
# 0.5.0 - добавлены тип шва start-to-end. Добавлен параметр robots для указания задействованных роботов
# 0.5.1 - высота бокового поиска равна половине высоты ребра
# 0.6.0 - добавлен параметр welding_sequence для указания порядка сварки
# 0.7.0 - reverse_deflection для указания высоты обратного прогиба

# requrements: numpy, transforms3d, pyyaml

# TODO ребра с дистанцией между ними меньше 200 мм. Возможно изменение угла сварки и поиска,
# использование поиска от соседних рёбер если там позволяет дистанция

import numpy as np
import krl_templates as k_tmpl
from coord_templates import CoordTemplates
import math
import transforms3d as t3d
import json
import os
import shutil
import logging
import yaml

# Константы
ZERO_3D = np.array([0, 0, 0])
MIN_SPEED = 24
MAX_SPEED = 39
MIN_AMP = 2
MAX_AMP = 3.5

class DetailParams:
    """
    Класс для хранения и обновления параметров детали.
    """
    def __init__(self, params):
        self.__dict__.update(params)

    def update(self, new_params):
        self.__dict__.update(new_params)

# Инициализация параметров детали по умолчанию
# Точкой отсчёта берётся верхний левый угол детали если стоять лицом к треку
detail_params = DetailParams({
    'robots': [1,2], # роботы, задействованные в сварке
    'd_type': 'd_slope_right',     # тип настила
    'edge_count': 2,         # количество рёбер
    't_p': 16,               # толщина настила
    't_r': 16,               # толщина ребра
    'l': 11166,              # длина настила
    'w': 2260,               # ширина настила
    'd_b1': 243,            # скос слева
    'd_b2': 243,            # скос справа
    'h': 200,                # высота ребра
    'd_w':[
        { # первое ребро
        "d_dis1": 300.0, # поперечное расстояние от края до первого ребра по левому краю
        "d_dis2": 300.0, # поперечное расстояние от края до первого ребра по правому краю
        "d_l1": 250.0, # продольное расстояние от края плиты до ребра по левому краю
        "d_l2": 250.0, # продольное расстояние от края плиты до ребра по правому краю
        "l_r1": 500.0, # роспуск слева
        "l_r2": 200.0  # роспуск справа
        },
        {
        "d_dis1": 600.0, # поперечное расстояние от предыдущего ребра по левому краю
        "d_dis2": 600.0, # поперечное расстояние от предыдущего ребра по правому краю
        "d_l1": 250.0,
        "d_l2": 250.0,
        "l_r1": 500.0,
        "l_r2": 200.0
        },
    ],
    #'d_l1': 801,             # расстояние до торца ребра слева 
    #'d_l2': 201,             # расстояние до торца ребра справа
    #'l_r1': 150,             # роспуск/отступ слева
    #'l_r2': 200,             # роспуск/отступ справа
    'd_s1': 60,              # отступ поиска слева
    'd_s2': 60,              # отступ поиска справа
    'd_t1': 3,               # технологический отступ начала шва
    'd_t2': 2,               # технологический отступ конца шва
    'l_overlap': 8,          # перекрытие швов
    'job': 1,                # номер сварочной программы
    'velocity': 31,          # скорость сварки
    'weld_gantry_radius': 1200, # плоский радиус фланец позиционера - фланец робота для сварки
    'search_gantry_radius': 1200, # плоский радиус фланец позиционера - фланец робота для поиска
    'base_displace': [0.0, 0.0, 0.0], # смещение базы (если деталь укладывается не в угол образованный упорами)
    'welding_sequence': None,
    'reverse_deflection':0
})

c_tmpl = CoordTemplates(detail_params.__dict__)

# Параметры инструмента
tool = {
    'XYZ': [-31, 0, 562],
    'CBA': [-1.86, -59.54, -0.1]
}

# координаты фланца позиционера при 0 положении осей
gantry_flange = np.array([0, 503, 1737])

# некоторые параметры позиционера для расчёта положения внешних осей
gantry_param = {
    'flange': gantry_flange,
    'E1_lim': [-10000, 10000],
    'E2_lim': [0, 0],
    'E3_lim': [-480, 1800],
    'E1_dir': 1,
    'E2_dir': 1,
    'E3_dir': 1,
}

base0 = np.array([10920, 0, 0])
base = base0.copy()

def calc_arc_z_offset(y_pos, total_width, deflection, base_offset):
    """
    Расчет смещения по Z для заданной координаты Y на дуге
    
    :param y_pos: текущая координата Y
    :param total_width: полная ширина детали
    :param deflection: величина прогиба
    :return: смещение по Z
    """
    if deflection == 0:
        return 0
        
    # Центр дуги
    y_center = total_width / 2
    
    # Радиус дуги
    # R = (L² + 4h²) / (8h), где L - хорда (ширина), h - высота (прогиб)
    radius = (total_width**2 + 4*deflection**2) / (8*deflection)
    
    # Смещение центра дуги по Z
    z_center = radius - deflection + base_offset[2]
    
    # Расчет Z для текущей точки на дуге
    # Z = √(R² - (y - y_center)²) - (R - h)
    z = math.sqrt(radius**2 - ((y_pos - base_offset[1]) - y_center)**2) - (radius - deflection)
    
    return z

def apply_deflection_to_point(point_params):
    """
    Применение обратного прогиба к точке
    
    :param point: исходная точка [x, y, z]
    :param detail_params: параметры детали
    :return: точка с учетом прогиба
    """
    if detail_params.reverse_deflection == 0:
        return
        
    width = detail_params.w
    # Для прямоугольных деталей ширина не задаётся, поэтому примерно считаем по рёбрам
    if width == 0 or detail_params.d_type == 'd_rect':
         width = sum([d['d_dis1'] for d in detail_params.d_w]) + detail_params.d_w[0]['d_dis1']

    point = point_params[0]
    
    # Рассчитываем смещение по Z
    z_offset = calc_arc_z_offset(
        point[1],  # Y координата
        width,  # Ширина детали
        detail_params.reverse_deflection,
        base
    )
    
    # Применяем смещение на месте
    point[2] += z_offset
    
    #return new_point

def apply_deflection_to_statement(statement):
    for point_param in statement:
        apply_deflection_to_point(point_param)



def interpolate_weld_parm_by_speed(a1, a2, speed):
    """
    Интерполяция параметров сварки в зависимости от скорости сварки.
    
    :param a1: угол горелки при минимальном катете
    :param a2: угол горелки при максимальном катете
    :param speed: скорость сварки
    :return: кортеж (угол горелки, длина волны, амплитуда)
    """
    i_spd = max(MIN_SPEED, min(speed, MAX_SPEED))
    proportion = (i_spd - MIN_SPEED) / (MAX_SPEED - MIN_SPEED)
    res_a = a1 + (a2 - a1) * (1 - proportion**2)
    wave_len = i_spd / (MAX_SPEED / 4.5)
    wave_amp = MIN_AMP + (MAX_AMP - MIN_AMP) * (1 - proportion)
    return res_a, wave_len, wave_amp

def gen_search_coord(start_point, displace, template, points=3):
    """
    Генерация последовательности точек поиска для одного ребра.
    
    :param start_point: начальная точка
    :param displace: смещение
    :param template: шаблон точек
    :param points: количество точек поиска
    :return: список точек и параметров движения к ним описывающих процедуру поиска
     [(координаты точки xyz), (углы cba), S, T, скорость, тип точки, дистанция для расчёта позиционера]
    """
    search_gantry_radius = detail_params.search_gantry_radius
    res = [
        [start_point + template['via'][0] + displace, *template['via'][1:4], 100, 'via', search_gantry_radius]
    ]
    
    search_points = {
        1: [('s1', 't1')],
        2: [('s1', 't1'), ('s2', 't2')],
        3: [('s1', 't1'), ('s2', 't2'), ('s3v1', 's3v2', 't3')]
    }
    
    for point_set in search_points.get(points, []):
        for point in point_set:
            res.append([
                start_point + template[point][0] + displace,
                *template[point][1:4],
                0.06 if 's' in point else 0.02,
                point,
                search_gantry_radius if 's' in point else 0
            ])
    
    res.append([start_point + template['departure'][0] + displace, *template['departure'][1:4], 0.1, 'departure', search_gantry_radius])
    return res

def gen_weld_coord(start_point, end_point, template, departure_displace=ZERO_3D):
    """
    Генерация последовательности точек шва для одного ребра.
    
    :param start_point: начальная точка шва
    :param end_point: конечная точка шва
    :param template: шаблон точек
    :param departure_displace: смещение точки отхода
    :return: список точек шва
    """
    #weld_vector = end_point - start_point
    #weld_len = np.linalg.norm(weld_vector)
    #weld_direction = weld_vector / weld_len
    
    weld_gantry_radius = detail_params.weld_gantry_radius
    angle, wl, wa = interpolate_weld_parm_by_speed(template['weld'][1], template['weld2'][1], detail_params.velocity)
    
    return [
        [start_point + template['via'][0], angle, *template['via'][2:4], 100, 'via', weld_gantry_radius],
        [start_point + template['near'][0], angle, *template['near'][2:4], 0.200, 'near', weld_gantry_radius],
        [start_point + template['weld'][0], angle, *template['weld'][2:4], 0.100, 'arcon', weld_gantry_radius],
        [end_point, angle, *template['weld'][2:4], 0.01, 'arcoff', weld_gantry_radius],
        #[end_point + weld_direction * np.linalg.norm(template['near'][0]), angle, *template['near'][2:4], 0.200, 'away', weld_gantry_radius],
        #[end_point + weld_direction * np.linalg.norm(template['via'][0]) + departure_displace, angle, *template['via'][2:4], 0.4, 'departure', weld_gantry_radius],
        [end_point + template['near'][0], angle, *template['near'][2:4], 0.200, 'away', weld_gantry_radius],
        [end_point + template['via'][0] + departure_displace, angle, *template['via'][2:4], 0.4, 'departure', weld_gantry_radius],
    ]

def robot_flange_mat_from_pos(pos, tool):
    """
    Расчёт матрицы положения фланца робота на основании текущей позиции (точки) и инструмента.
    
    :param pos: текущая позиция
    :param tool: параметры инструмента
    :return: матрица положения фланца робота
    """
    tool_rot_m = t3d.euler.euler2mat(*map(math.radians, tool['CBA']), 'sxyz')
    tool_m = t3d.affines.compose(tool['XYZ'], tool_rot_m, np.ones(3))
    inv_tool_m = np.linalg.inv(tool_m)
    
    pos_rot_m = t3d.euler.euler2mat(*map(math.radians, pos[1]), 'sxyz')
    pos_m = t3d.affines.compose(pos[0], pos_rot_m, np.ones(3))
    
    return np.dot(pos_m, inv_tool_m)

def xyz_cba_from_mat(m):
    """
    Получение координат и углов в формате XYZCBA из матрицы поворота.
    
    :param m: матрица поворота
    :return: кортеж (xyz, cba)
    """
    d = t3d.affines.decompose(m)
    xyz = d[0]
    cba = list(map(math.degrees, t3d.euler.mat2euler(d[1], 'sxyz')))
    return xyz, cba

def calc_gantry_for_pos(pos, gantry_params, prev_pos):
    """
    Расчёт положения внешних осей для заданной точки.
    
    :param pos: текущая точка
    :param gantry_params: параметры позиционера
    :param prev_pos: предыдущая точка
    :return: список положений внешних осей [e1, e2, e3]
    """
    
    # Дистанция в плоскости XY между фланцем робота и фланцем позиционера
    # Если указано 0, то берётся дистанция предыдущей точки (на поиске лучше не двигать внешние оси).
    optimal_flat_radius = pos[6] 
    if optimal_flat_radius == 0 and len(prev_pos) > 6:
        return prev_pos[7]

    fm = robot_flange_mat_from_pos(pos, tool)
    xyz, cba = xyz_cba_from_mat(fm)
    x, y, z = xyz
    
    e1 = (x - optimal_flat_radius) * gantry_params['E1_dir']
    e2 = 0
    wy = pos[0][1]
    e3 = (wy - gantry_params['flange'][1]) * gantry_params['E3_dir']
    
    e3 += 200 if cba[2] > 0 else -200
    
    if abs(e3) > gantry_params['E3_lim'][1]:
        d = abs(e3) - gantry_params['E3_lim'][1]
        if d < optimal_flat_radius:
            d1 = (optimal_flat_radius - math.sqrt(optimal_flat_radius**2 - d**2)) * gantry_params['E1_dir']
            e1 += d1
        else:
            e1 = x * gantry_params['E1_dir']
    
    e1 = max(min(e1, gantry_params['E1_lim'][1]), gantry_params['E1_lim'][0])
    e2 = max(min(e2, gantry_params['E2_lim'][1]), gantry_params['E2_lim'][0])
    e3 = max(min(e3, gantry_params['E3_lim'][1]), gantry_params['E3_lim'][0])
    
    return [e1, e2, e3]

def gen_gantry(statement):
    """
    Добавление положения внешних осей для всех точек в последовательности.
    
    :param statement: последовательность точек
    """
    prev_pos = statement[0]
    for pos in statement:
        #apply_deflection_to_point(pos) # обратный прогиб детали (с точки зрения логики должно быть где-то в другом месте)
        ext_ax = calc_gantry_for_pos(pos, gantry_param, prev_pos)
        pos.append(ext_ax)
        prev_pos = pos

def gen_seq_one_seam(edge_num, side, seam_type):
    """
    Генерация последовательности для одного сварного шва 
    в зависимости от типа (начало-середина или середина-конец)
    и стороны ребра с которой накладывается шов.
    
    :param edge_num: номер ребра
    :param side: сторона (left или right)
    :param seam_type: тип шва (start-to-middle, middle-to-end, start-to-end)
    :return: список операций для шва
    """
    # Передаём в шаблон номер текущего ребра для выбора правильных отступов на поиске
    c_tmpl.current_edge = edge_num 
    
    # Извлечение параметров детали
    d_type = detail_params.d_type # тип настила d_rect, d_slope_left, d_slope_right, d_trapezoid_top, d_trapezoid_bottom
    #edge_count = detail_params.edge_count # количество рёбер
    t_p = detail_params.t_p # толщина настила
    t_r = detail_params.t_r # толщина ребра
    l = detail_params.l  # длина настила
    w = detail_params.w # ширина настила
    h = detail_params.h # высота ребра
    d_w = detail_params.d_w # список с параметрами отдельных рёбер
    #d_s1 = detail_params.d_s1 # отступ поиска слева
    #d_s2 = detail_params.d_s2 # отступ поиска справа
    d_t1 = detail_params.d_t1 # технологический отступ начала шва
    d_t2 = detail_params.d_t2 # технологический отступ конца шва
    l_overlap = detail_params.l_overlap # перекрытие швов

    # Извлечение параметров конкретного ребра
    d_l1 = d_w[edge_num]['d_l1'] # расстояние до торца ребра слева 
    d_l2 = d_w[edge_num]['d_l2'] # расстояние до торца ребра справа
    l_r1 = d_w[edge_num]['l_r1'] # роспуск/отступ слева
    l_r2 = d_w[edge_num]['l_r2'] # роспуск/отступ справа


    # Применение параметров "скос" в зависимости от выбранной формы настила
    # corner1 - левый верхний угол
    # corner2 - правый верхний угол
    # corner3 - левый нижний угол
    # corner4 - правый нижний угол
    # 1-------2
    #  \       \
    #   \       \
    #    3-------4

    corner1 = corner2 = corner3 = corner4 = 0
    if d_type == 'd_slope_left': # наклон влево
        corner2 = detail_params.d_b2
        corner3 = detail_params.d_b1
    elif d_type == 'd_slope_right': # наклон вправо
        corner1 = detail_params.d_b1
        corner4 = detail_params.d_b2
    elif d_type == 'd_trapezoid_top': # трапеция
        corner1 = detail_params.d_b1
        corner2 = detail_params.d_b2
    elif d_type == 'd_trapezoid_bottom': # перевёрнутая трапеция
        corner3 = detail_params.d_b1
        corner4 = detail_params.d_b2

    # Расчет поперечного положения ребра (координата Y)
    r_y_pos_left = sum([d['d_dis1'] for d in d_w[:edge_num + 1]]) # слева
    r_y_pos_right = sum([d['d_dis2'] for d in d_w[:edge_num + 1]]) # справа
    r_y_pos_mid = (r_y_pos_left + r_y_pos_right) / 2 # по центру

    # Расчет продольного изменения длины ребра и его положения для непрямоугольного настила
    bevel1 = bevel2 = 0
    if d_type != 'd_rect' and w != 0:
        bevel1 = corner1 + ((corner3 - corner1) / w) * r_y_pos_left
        bevel2 = corner2 + ((corner4 - corner2) / w) * r_y_pos_right

    # Расчет длины шва = <длина настила> - <дистанция до начала ребра> - <роспуски> - <скосы>
    #s_len = l - d_l1 - d_l2 - l_r1 - l_r2 - (bevel1 + bevel2)
    # Длина ребра = <длина настила> - <дистанции до начала ребра> - <скосы>
    r_len = l - (d_l1 + d_l2) - (bevel1 + bevel2)
    logging.info(f'Edge {edge_num} len: {r_len}, bevel1: {bevel1}, bevel2: {bevel2}')
    statements = []
    start_point = None
    end_point = None

    # Генерация последовательности команд/точек в зависимости от типа шва и стороны
    if seam_type == 'middle-to-end':
        # Расчет координаты X начала и конца шва для варианта середина-конец
        # начало = скос настила + расстояние до торца ребра слева + половина длины ребра - перекрытие швов
        seam_x_pos_start = -(bevel1 + d_l1 + r_len/2 - l_overlap)
        # конец = скос настила + расстояние до торца ребра слева + длина ребра - роспуск справа - тех. отступ конца
        seam_x_pos_end = -(bevel1 + d_l1 + r_len - l_r2 - d_t2)
        
        if side == 'left':
            # Определение точек начала и конца шва для левой стороны
            start_point = np.array([seam_x_pos_start, r_y_pos_mid - t_r/2, t_p]) + base
            end_point = np.array([seam_x_pos_end, r_y_pos_right - t_r/2, t_p]) + base
            
            # Генерация последовательности операций для левой стороны
            statements.append(('CorrOff', None, None))
            s_left_end = gen_search_coord(end_point, np.zeros(3), c_tmpl.search_template_left_end)
            #gen_gantry(s_left_end)
            statements.append(('SE', edge_num, s_left_end))
            s_left_start = gen_search_coord(start_point, np.zeros(3), c_tmpl.search_template_left_start, 2)
            #gen_gantry(s_left_start)
            statements.append(('SS', edge_num, s_left_start))
            w_left = gen_weld_coord(start_point, end_point, c_tmpl.weld_template_left, np.array([-l_r2 - 200, 0, 0]))
            #gen_gantry(w_left)
            statements.append(('W', edge_num, w_left))
            
        elif side == 'right':
            # Определение точек начала и конца шва для правой стороны
            start_point = np.array([seam_x_pos_start, r_y_pos_mid + t_r/2, t_p]) + base
            end_point = np.array([seam_x_pos_end, r_y_pos_right + t_r/2, t_p]) + base
            
            # Генерация последовательности операций для правой стороны
            statements.append(('CorrOff', None, None))
            s_right_end = gen_search_coord(end_point, np.zeros(3), c_tmpl.search_template_right_end)
            #gen_gantry(s_right_end)
            statements.append(('SE', edge_num, s_right_end))
            s_right_start = gen_search_coord(start_point, np.zeros(3), c_tmpl.search_template_right_start, 2)
            #gen_gantry(s_right_start)
            statements.append(('SS', edge_num, s_right_start))
            w_right = gen_weld_coord(start_point, end_point, c_tmpl.weld_template_right, np.array([-l_r2 - 200, 0, 0]))
            #gen_gantry(w_right)
            statements.append(('W', edge_num, w_right))
                                    
    elif seam_type == 'start-to-middle' or 'start-to-end':
        # Расчет координаты X начала и конца шва для варианта начало-середина
        # начало = скос настила + расстояние до торца ребра слева + роспуск слева + тех. отступ начала
        seam_x_pos_start = -(bevel1 + d_l1 + l_r1 + d_t1)
        
        if seam_type == 'start-to-middle':
            # конец = скос настила + расстояние до торца ребра слева + половина длины ребра
            seam_x_pos_end = -(bevel1 + d_l1 + r_len/2)
        else: # start-to-end
            # конец = скос настила + расстояние до торца ребра слева + длина ребра - роспуск справа - тех. отступ конца
            seam_x_pos_end = -(bevel1 + d_l1 + r_len - l_r2 - d_t2)

        
        if side == 'left':
            # Определение точек начала и конца шва для левой стороны
            start_point = np.array([seam_x_pos_start, r_y_pos_left - t_r/2, t_p]) + base
            end_point = np.array([seam_x_pos_end, r_y_pos_mid - t_r/2, t_p]) + base
            
            # Генерация последовательности операций для левой стороны
            statements.append(('CorrOff', None, None))
            s_left_start = gen_search_coord(start_point, np.zeros(3), c_tmpl.search_template_left_start)
            #gen_gantry(s_left_start)
            statements.append(('S', edge_num, s_left_start))
            w_left = gen_weld_coord(start_point, end_point, c_tmpl.weld_template_left)
            #gen_gantry(w_left)
            statements.append(('W', edge_num, w_left))
            
        elif side == 'right':
            # Определение точек начала и конца шва для правой стороны
            start_point = np.array([seam_x_pos_start, r_y_pos_left + t_r/2, t_p]) + base
            end_point = np.array([seam_x_pos_end, r_y_pos_mid + t_r/2, t_p]) + base

            # Генерация последовательности операций для правой стороны
            statements.append(('CorrOff', None, None))
            s_right_start = gen_search_coord(start_point, np.zeros(3), c_tmpl.search_template_right_start)
            #gen_gantry(s_right_start)
            statements.append(('S', edge_num, s_right_start))
            w_right = gen_weld_coord(start_point, end_point, c_tmpl.weld_template_right)
            #gen_gantry(w_right)
            statements.append(('W', edge_num, w_right))
    
    for s_type, e_num, statement in statements:
        if statement is not None:
            apply_deflection_to_statement(statement)
            gen_gantry(statement)

    return statements

def write_ptp(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax):
    """
    Запись PTP (Point-to-Point) движения в src и dat файлы.
    """
    src_f.write(k_tmpl.ptp_src_template.format(p_name, p_speed))
    dat_f.write(k_tmpl.ptp_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax))

def write_search(src_f, dat_f, s_type, s_number, i, statement):
    """
    Запись операции поиска в src и dat файлы.
    """
    p = statement[i]
    p2 = statement[i+1]
    p_xyz, p_cba, p_S, p_T, p_speed, p_type, p_gantry, p_ext_ax = p
    p2_xyz, p2_cba, p2_S, p2_T, p2_speed, p2_type, p2_gantry, p2_ext_ax = p2
    p_name = f'{s_type}{s_number+1}_{i+1}'
    p2_name = f'{s_type}{s_number+1}_{i+2}'
    src_f.write(k_tmpl.search_src_template.format(p_name, p_speed, p2_name))
    dat_f.write(k_tmpl.search_dat_template.format(
        p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax,
        p2_name, p2_speed, *p2_xyz, *reversed(p2_cba), p2_S, p2_T, *p2_ext_ax,
    ))
    cd0 = 'TSg_CD0'
    if p_type == 's1':
        cd1 = f'CD{s_type}{s_number+1}_{i+1}'
        src_f.write(k_tmpl.corr_src_template_1d.format(1, cd1, cd0, cd0, cd0, cd0, cd0))
    if p_type == 's2':
        cd1 = f'CD{s_type}{s_number+1}_{i+1-2}'
        cd2 = f'CD{s_type}{s_number+1}_{i+1}'
        src_f.write(k_tmpl.corr_src_template_2d.format(2, cd1, cd0, cd0, cd2, cd0, cd0))
    elif p_type == 's3v2':
        cd1 = f'CD{s_type}{s_number+1}_{i+1-5}'
        cd2 = f'CD{s_type}{s_number+1}_{i+1-3}'
        cd3 = f'CD{s_type}{s_number+1}_{i+1}'
        src_f.write(k_tmpl.corr_src_template_3d.format(3, cd1, cd0, cd0, cd2, cd0, cd3))

def write_arcon(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax, job, velocity):
    """
    Запись операции включения дуги (ARCON) в src и dat файлы.
    """
    angle, wl, wa = interpolate_weld_parm_by_speed(0, 0, velocity)
    src_f.write(f'\ncustom_sync(\'h{250:02X}{250:02X}0000\', \'hffff0000\', {250})\n')
    src_f.write(k_tmpl.arcon_src_template.format(p_name, p_speed))
    dat_f.write(k_tmpl.arcon_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax, job, velocity/6000, wl, wa))

def write_arcoff(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax, job, velocity):
    """
    Запись операции выключения дуги (ARCOFF) в src и dat файлы.
    """
    src_f.write(k_tmpl.arcoff_src_template.format(p_name, p_speed))
    dat_f.write(k_tmpl.arcoff_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax, job, velocity/6000))

def write_lin(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax):
    """
    Запись линейного движения (LIN) в src и dat файлы.
    """
    src_f.write(k_tmpl.lin_src_template.format(p_name, p_speed))
    dat_f.write(k_tmpl.lin_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax))

def write_out_statement(src_f, dat_f, s_type, s_number, statement):
    """
    Запись операции в src и dat файлы.
    
    :param src_f: файл исходного кода
    :param dat_f: файл данных
    :param s_type: тип операции
    :param s_number: номер операции
    :param statement: данные операции
    """
    job = detail_params.job
    velocity = detail_params.velocity
    
    if s_type == 'CorrOff':
        src_f.write('TS_CorrOff()')
        return
    
    for i, p in enumerate(statement):
        p_xyz, p_cba, p_S, p_T, p_speed, p_type, p_gantry, p_ext_ax = p
        p_name = f'{s_type}{s_number+1}_{i+1}'
        
        if p_type == 'via':
            write_ptp(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax)
        elif p_type in ['s1', 's2', 's3v2']:
            write_search(src_f, dat_f, s_type, s_number, i, statement)
        elif p_type == 'arcon':
            write_arcon(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax, job, velocity)
        elif p_type == 'arcoff':
            write_arcoff(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax, job, velocity)
        elif p_type not in ['t1', 't2', 't3']:
            # t1 t2 t3 игнорируем, это референсные точки для поиска
            write_lin(src_f, dat_f, p_name, p_speed, p_xyz, p_cba, p_S, p_T, p_ext_ax)

            

def write_seam_src_dat(edge_num, side, seam_type, folder='d:\\'):
    """
    Запись файлов src и dat для одного шва.
    
    :param edge_num: номер ребра
    :param side: сторона ребра относительно направления сварки (left или right)
    :param seam_type: тип шва (start-to-middle, middle-to-end, start-to-end)
    :param folder: папка для сохранения файлов
    :return: имя программы и список операций
    """
    st = ''.join([w[0] for w in seam_type.split('-')])
    prog_name = f'edge_{edge_num}_{side}_{st}'
    statements = gen_seq_one_seam(edge_num, side, seam_type)
    with open(os.path.join(folder, f'{prog_name}.src'), 'w') as src_f:
        src_f.write(k_tmpl.src_header_template.format(prog_name))
        with open(os.path.join(folder, f'{prog_name}.dat'), 'w') as dat_f:
            dat_f.write(k_tmpl.dat_header_template.format(prog_name))
            for i, statement_rec in enumerate(statements):
                s_type, ii, statement = statement_rec    
                write_out_statement(src_f, dat_f, s_type, i, statement)
            dat_f.write(k_tmpl.dat_footer_template)
        src_f.write('\nTS_CorrOff()\n')
        src_f.write('ochistka_mod()\n')
        src_f.write(k_tmpl.src_footer_template)
    return prog_name, statements

def generate(out_dir, main_name, robot_num=1, seam_type='start-to-end'):
    """
    Вызов функции генерации и записи подпрограмм для каждого шва.
    Сборка полученных подпрограмм в общее "меню" (главный запускаемый файл).
    
    :param out_dir: директория вывода
    :param main_name: имя основного файла
    :param robot_num: номер робота
    :param seam_type: тип шва (start-to-middle, middle-to-end, start-to-end)
    """
    if not out_dir:
        out_dir = ''
    if not main_name:
        main_name = 'main_gen.src'
    out_dir_rob = os.path.join(out_dir, f'R{robot_num}')
    shutil.rmtree(out_dir_rob, ignore_errors=True)
    os.makedirs(out_dir_rob)

    edge_count = detail_params.edge_count
    d_w = detail_params.d_w
    if edge_count != len(d_w):
        logging.info('Warning: edge_count != len(d_w)')
        edge_count = len(d_w)
        detail_params.edge_count = edge_count

    welding_sequence = detail_params.welding_sequence
    if not welding_sequence: # если не задан порядок сварки, то формируем по умолчанию
        welding_sequence = []
        edge_seqence = [i for i in range(edge_count) if i % 2] + [i for i in range(edge_count) if not i % 2]
        for side in ['right_side', 'left_side']:
            for edge_num in edge_seqence:
                welding_sequence.append((edge_num+1, side))
        
    
    main_f = os.path.join(out_dir_rob, main_name)
    with open(main_f, 'w') as f:
        f.write(k_tmpl.src_header_template.format('main_gen'))
        f.write('\nunfold()\n')
        f.write('\ntcptrack_mod()\n')
        f.write('ochistka_mod()\n\n')
        sync = 1
        for edge_num, side in welding_sequence:
            side = side.replace('_side', '')
            prog_name, statements = write_seam_src_dat(edge_num-1, side, seam_type, out_dir_rob)
            f.write(f'custom_sync(\'h{sync:02X}{sync:02X}0000\', \'hffff0000\', {sync})\n')
            f.write(f'{prog_name}()\n\n')
            sync += 1

        #for side in ['right', 'left']:
        #    for edge_num in edge_seqence:
        #        prog_name, statements = write_seam_src_dat(edge_num, side, seam_type, out_dir_rob)
        #        f.write(f'custom_sync(\'h{sync:02X}{sync:02X}0000\', \'hffff0000\', {sync})\n')
        #        f.write(f'{prog_name}()\n\n')
        #        sync += 1

        f.write(f'custom_sync(\'h{sync:02X}{sync:02X}0000\', \'hffff0000\', {sync})\n')
        f.write('\ngo_end()\n')
        f.write('\ngo_home()\n')
        f.write('\nfold()\n')
        f.write(k_tmpl.src_footer_template)
        e1_home = -11400 if robot_num != 1 else -6000
        f.write(k_tmpl.def_go_end_template.format(e1_home))
        f.write(k_tmpl.def_go_home_template.format(e1_home))

def main():
    global base, base0, detail_params
    """
    Стартовая функция. 
    Инициализация параметров, логирование и запуск генерации.
    """
    try:
        import argparse
        import sys
        import traceback
        logging.basicConfig(filename='generator.log',    
                            format="[%(asctime)s] %(levelname)s %(message)s",
                            datefmt="%d/%b/%Y %H:%M:%S", level=logging.INFO)
        parser = argparse.ArgumentParser(description="welding KRL generator")
        parser.add_argument("-p", dest="detail_params")
        parser.add_argument("-o", dest="output_dir")
        parser.add_argument("-n", dest="main_name")
        args, leftovers = parser.parse_known_args()
        logging.info(f'Script version: {__version__}')
        logging.info('Start generating: ' + ' '.join(sys.argv))
        if args.detail_params:
            with open(args.detail_params, 'r') as f:
                new_params = json.load(f)
                detail_params.update(new_params)
        logging.info('detail_params:\n' + yaml.dump(detail_params.__dict__, default_flow_style=False))
        detail_params.base_displace[0] = -detail_params.base_displace[0]
        base = base0 + np.array(detail_params.base_displace)
        #for r in range(2):
        for r in detail_params.robots:
            if len(detail_params.robots) == 1:
                seam_type = 'start-to-end'
            elif r == 1:
                seam_type = 'start-to-middle'   
            else:
                seam_type = 'middle-to-end'
            generate(args.output_dir, args.main_name, r, seam_type)
    except Exception as e:
        logging.error(traceback.format_exc())
        print(traceback.format_exc())

if __name__ == '__main__':
    if True:
        main()
    else:
        # Визуализация графа вызова функций. Отладочная часть, можно выкинуть
        from pycallgraph import PyCallGraph, Config, GlobbingFilter
        from pycallgraph.output import GraphvizOutput
        config = Config()
        config.trace_filter = GlobbingFilter(exclude=[
            'pycallgraph.*',
            '_*',
            '*FileFinder*',
            '*module*',
            'SourceFileLoader*',
        ])
        with PyCallGraph(output=GraphvizOutput(), config=config):
            main()