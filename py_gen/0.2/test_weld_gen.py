__version__ = '0.2.0'
# 0.1.0 - начальная версия простые прямоугольные детали
# 0.2.0 - добавлены детали с наклоном, трапециевидные детали, изменение параметров сварки в зависимости от скорости

import numpy as np
import krl_templates as k_tmpl
#import coord_templates as c_tmpl
from coord_templates import CoordTemplates
import math
import transforms3d as t3d
import json
import os, shutil

# лево - право определяется по направлению сварки
# продольное направление сварки в -X

# "начало" считаем от привязки
# TODO пересчитать параметры в 0 в зависимости от привязки на чертеже

# возможно отказаться от привязки и указывать от левого верхнего угла
# размеры на чертежах обычно присутствуют
detail_params = {
    'edge_count':5, # количество рёбер
    't_p':16,       # толщина настила
    't_r':16,       # толщина ребра
    'l':12007,      # длина детали
    #'w':2260,       # ширина детали
    'h':200,        # высота ребра
    'd_w1':297,     # расстояние по ширине до осевой линии первого ребра
    'd_w2':600,     # расстояние между осевыми линиями рёбер
    'd_l1':196,     # расстояние до торца ребра слева 
    'd_l2':196,     # расстояние до торца ребра справа
    'l_r1':150,     # роспуск в начале
    'l_r2':150,     # роспуск в конце
    'd_s1':60,      # отступ поиска в начале
    'd_s2':60,      # отступ поиска в конце
    'd_t1':3,       # технологический отступ начала шва
    'd_t2':2,       # технологический отступ конца шва
    'l_overlap':5,  # перекрытие швов
    'job':1,        # номер сварочной программы
    'velocity': 24  # скорость сварки
}


c_tmpl = CoordTemplates(detail_params)


tool = {
    'XYZ':[-74.291, 1.638, 654.528],
    'CBA':[0.089, -68.311, -0.089]
}
tool = {
    'XYZ':[-79.03, 3.7, 660.37],
    'CBA':[-3.97, -68.42, 1.59]
}
# координаты фланца позиционера при 0 положении осей
gantry_flange = np.array([0, 503, 1737])

# некоторые параметры позиционера для расчёта положения внешних осей
gantry_param = {
    'flange':gantry_flange,
    'E1_lim':[-10000, 10000],
    'E2_lim':[0,0],
    'E3_lim':[-480,1800],
    'E1_dir':1,
    'E2_dir':1,
    'E3_dir':1,
    #'flat_radius':900 # оптимальный радиус работы в плоскости XY 
}

# смещение базы = 0 детали
#base = np.array([4000, 915, 200])
base = np.array([10920, 0, 0])





def gen_search_coord(start_point, displace, template, points = 3):
    d = displace
    res = [ ]
    res.append([start_point + template['via'][0] + d, *template['via'][1:4], 100, 'via', 1100])
    # в зависимости от кол-ва точек поиска генерируем выходной массив
    if points > 0:
        pts = [
            [start_point + template['s1'][0] + d, *template['s1'][1:4], 0.06, 's1', 1100],
            [start_point + template['t1'][0] + d, *template['t1'][1:4], 0.02, 't1', 0]
        ]
        res += pts
    if points > 1:
        pts = [
            [start_point + template['s2'][0] + d, *template['s2'][1:4], 0.06, 's2', 1100],
            [start_point + template['t2'][0] + d, *template['t2'][1:4], 0.02, 't2', 0]
        ]
        res += pts
    if points > 2:
        pts = [
            [start_point + template['s3v1'][0] + d, *template['s3v1'][1:4], 0.06, 's3v1', 1100],
            [start_point + template['s3v2'][0] + d, *template['s3v2'][1:4], 0.02, 's3v2', 0],
            [start_point + template['t3'][0] + d, *template['t3'][1:4], 0.1, 't3', 0]
        ]
        res += pts
    res.append([start_point + template['departure'][0] + d, *template['departure'][1:4], 0.1, 'departure', 1100])
    # (x,y,z), (c,b,a), S, T, speed, type, gantry
    return res
    

def gen_weld_coord(start_point, weld_len, template):
    d = np.array([-weld_len, 0, -1])
    res = [
        [start_point + template['via'][0], *template['via'][1:4], 100, 'via', 900],
        [start_point + template['near'][0], *template['near'][1:4], 0.200, 'near', 900],
        [start_point + template['weld'][0], *template['weld'][1:4], 0.100, 'arcon', 900],
        [start_point + template['weld'][0] + d, *template['weld'][1:4], 0.01, 'arcoff', 900],
        [start_point + template['near'][0] + d, *template['near'][1:4],0.200 , 'away', 900],
        [start_point + template['via'][0] + d, *template['via'][1:4], 0.4, 'departure', 900],
    ]
    # (x,y,z), (c,b,a), S, T, speed, type, gantry
    return res

def robot_flange_mat_from_pos(pos, tool):
    tool_rot_m = t3d.euler.euler2mat(*map(math.radians,tool['CBA']), 'sxyz')
    tool_m = t3d.affines.compose(tool['XYZ'], tool_rot_m, np.ones(3))
    inv_tool_m = np.linalg.inv(tool_m)
    
    pos_rot_m = t3d.euler.euler2mat(*map(math.radians,pos[1]), 'sxyz')
    pos_m = t3d.affines.compose(pos[0], pos_rot_m, np.ones(3))
    
    return np.dot(pos_m, inv_tool_m)
    
def xyz_cba_from_mat(m):
    d = t3d.affines.decompose(m)
    xyz = d[0]
    cba = list(map(math.degrees, t3d.euler.mat2euler(d[1],'sxyz')))
    return xyz, cba
    

def calc_gantry_for_pos(pos, gantry_params, prev_pos):
    #optimal_flat_radius = gantry_params['flat_radius']
    optimal_flat_radius = pos[6]
    if optimal_flat_radius == 0 and len(prev_pos) > 6:
        e1,e2,e3 = prev_pos[7]
        res = [e1, e2, e3]
        return res
    #x,y,z = pos[0]
    fm = robot_flange_mat_from_pos(pos, tool)
    xyz, cba = xyz_cba_from_mat(fm) # положение фланца
    x,y,z = xyz
    e1 = x * gantry_params['E1_dir'] - optimal_flat_radius * gantry_params['E1_dir']
    e2 = 0
    #e3 = y * gantry_params['E3_dir'] - gantry_params['flange'][1] * gantry_params['E3_dir']
    wy = pos[0][1] # Y координата шва
    e3 = wy * gantry_params['E3_dir'] - gantry_params['flange'][1] * gantry_params['E3_dir']
    d = 0
    d1 = 0
    if abs(e3) > gantry_params['E3_lim'][1]:
        d = abs(e3) - gantry_params['E3_lim'][1]
        if d < optimal_flat_radius:
            d1 = (optimal_flat_radius - math.sqrt(optimal_flat_radius**2 - d**2)) * gantry_params['E1_dir']
            e1 = e1 + d1
        else:
            e1 = x * gantry_params['E1_dir']
        
    #if gantry_params['E3_lim'][1] * gantry_params['E3_dir'] - e3 > optimal_flat_radius
    #res = [-x+770,0,y-1250]
    e1 = max(min(e1, gantry_params['E1_lim'][1]), gantry_params['E1_lim'][0])
    e2 = max(min(e2, gantry_params['E2_lim'][1]), gantry_params['E2_lim'][0])
    e3 = max(min(e3, gantry_params['E3_lim'][1]), gantry_params['E3_lim'][0])
    res = [e1, e2, e3]
    #print(res, d, d1)
    return res
    
def gen_gantry(statement):
    # добавление положения внешних осей
    prev_pos = statement[0]
    for pos in statement:
        ext_ax = calc_gantry_for_pos(pos, gantry_param, prev_pos)
        # (x,y,z), (c,b,a), S, T, speed, type, gantry, [e1, e2, e3]
        pos.append(ext_ax)
        prev_pos = pos



def gen_seq_one_seam(edge_num, side, seam_type):

    edge_count = detail_params['edge_count']
    t_p = detail_params['t_p']
    t_r = detail_params['t_r']
    l = detail_params['l']
    #w = detail_params['w']
    h = detail_params['h']
    d_w1 = detail_params['d_w1']
    d_w2 = detail_params['d_w2']
    d_l1 = detail_params['d_l1']
    d_l2 = detail_params['d_l2']
    l_r1 = detail_params['l_r1']
    l_r2 = detail_params['l_r2']
    d_s1 = detail_params['d_s1']
    d_s2 = detail_params['d_s2']
    d_t1 = detail_params['d_t1']
    d_t2 = detail_params['d_t2']
    l_overlap = detail_params['l_overlap']

    r_len = l - d_l1 - d_l2 - l_r1 - l_r2
    statements = []
    start_point = None
    if seam_type == 'middle-to-end':
        if side == 'left':
            # стартовая точка для шва от центра до конца детали слева от ребра
            # X = -(отступ от начала детали + роспуск + половина длины шва - расстояние на перекрытие швов)
            # Y = (поперечный отступ до первого ребра + расстояние между рёбрами * номер ребра +- толщина ребра/2)
            # Z = тощина плиты
            # + координата 0 детали
            start_point = np.array([-d_l1 - l_r1 - r_len/2 + l_overlap, 
                                    d_w1 + d_w2 * edge_num - t_r/2, 
                                    t_p]) + base
            # левый поиск у конца шва, 3 точки
            statements.append(('CorrOff', None, None))
            s_left_end = gen_search_coord(start_point, 
                                          np.array([-r_len/2 - l_overlap, 0, 0]), 
                                          c_tmpl.search_template_left_end)
            gen_gantry(s_left_end)
            statements.append(('SE', edge_num, s_left_end))
            # левый поиск у начала шва, 2 точки
            s_left_start = gen_search_coord(start_point, np.zeros(3), 
                                            c_tmpl.search_template_left_start, 2)
            gen_gantry(s_left_start)
            statements.append(('SS', edge_num, s_left_start))
            # левый шов. длина = половине полной длины + перекрытие - технологический отступ
            w_left = gen_weld_coord(start_point, r_len/2 + l_overlap - d_t2, c_tmpl.weld_template_left)
            gen_gantry(w_left)
            statements.append(('W', edge_num, w_left))
            
        elif side == 'right':
            # стартовая точка для шва от центра до конца детали справа от ребра
            start_point = np.array([-d_l1 - l_r1 - r_len/2 + l_overlap, 
                                    d_w1 + d_w2 * edge_num + t_r/2, 
                                    t_p]) + base
            # правый поиск у конца шва, 3 точки
            statements.append(('CorrOff', None, None))
            s_right_end = gen_search_coord(start_point, 
                                           np.array([-r_len/2 - l_overlap, 0, 0]), 
                                           c_tmpl.search_template_right_end)
            gen_gantry(s_right_end)
            statements.append(('SE', edge_num, s_right_end))
            # правый поиск у начала шва, 2 точки
            s_right_start = gen_search_coord(start_point, 
                                             np.zeros(3), 
                                             c_tmpl.search_template_right_start, 2)
            gen_gantry(s_right_start)
            statements.append(('SS', edge_num, s_right_start))
            # правый шов. длина = половине полной длины + перекрытие - технологический отступ
            w_right = gen_weld_coord(start_point, 
                                     r_len/2 + l_overlap - d_t2, 
                                     c_tmpl.weld_template_right)
            gen_gantry(w_right)
            statements.append(('W', edge_num, w_right))
                                    
                                    
    elif seam_type == 'start-to-middle':
        if side == 'left':
            # стартовая точка для шва от начала до центра детали слева от ребра
            start_point = np.array([-d_l1 - l_r1, 
                                    d_w1 + d_w2 * edge_num - t_r/2, 
                                    t_p]) + base
            # левый поиск у начала шва, 3 точки
            statements.append(('CorrOff', None, None))
            s_left_start = gen_search_coord(start_point, 
                                            np.zeros(3), 
                                            c_tmpl.search_template_left_start)
            gen_gantry(s_left_start)
            statements.append(('S', edge_num, s_left_start))
            # левый шов. К координатам начала добавлен технологический отступ. Длина = половина от полного шва
            w_left = gen_weld_coord(start_point - np.array([d_t1,0,0]), 
                                    r_len/2 - d_t1, 
                                    c_tmpl.weld_template_left)
            gen_gantry(w_left)
            statements.append(('W', edge_num, w_left))
        elif side == 'right':
            # стартовая точка для шва от начала до центра детали справа от ребра
            start_point = np.array([-d_l1 - l_r1, 
                                    d_w1 + d_w2 * edge_num + t_r/2, 
                                    t_p]) + base
            # правый поиск у начала шва, 3 точки
            statements.append(('CorrOff', None, None))
            s_right_start = gen_search_coord(start_point, 
                                             np.zeros(3), 
                                             c_tmpl.search_template_right_start)
            gen_gantry(s_right_start)
            statements.append(('S', edge_num, s_right_start))
            # правый шов. К координатам начала добавлен технологический отступ. Длина = половина от полного шва
            w_right = gen_weld_coord(start_point - np.array([d_t1,0,0]), 
                                     r_len/2 - d_t1, 
                                     c_tmpl.weld_template_right)
            gen_gantry(w_right)
            statements.append(('W', edge_num, w_right))
    return statements
    

def write_out_point(src_f, dat_f, p_name, pos, p_next_name = None, p_next = None):
    p_type = pos[5]
    speed = pos[4]
    # point name, speed 
    if p_type == 'via':
        src_template = k_tmpl.ptp_src_template
        dat_template = k_tmpl.ptp_dat_template
    else:
        src_template = k_tmpl.lin_src_template
        dat_template = k_tmpl.lin_dat_template
        
    src_f.write(src_template.format(p_name, speed))
    ext_ax = pos[7]
    dat_f.write(dat_template.format(p_name, speed, *pos[0], *reversed(pos[1]), pos[2], pos[3], *ext_ax))
    
# Запись команды в src и dat файл
# src_f, dat_f ссылки на открытые для записи файлы
# s_number номер команды в последовательности команд
# statement список точек внутри команды
#def write_out_statement(src_f, dat_f, s_type, s_number1, s_number2, statement):
def write_out_statement(src_f, dat_f, s_type, s_number, statement):
    job = detail_params['job']
    velocity = detail_params['velocity']
    if s_type == 'CorrOff':
        src_f.write('TS_CorrOff()')
        return
    for i, p in enumerate(statement):
        # (x,y,z), (c,b,a), S, T, speed, type, gantry, [e1, e2, e3]
        p_xyz, p_cba, p_S, p_T, p_speed, p_type, p_gantry, p_ext_ax = p
        p_name = '{0}{1}_{2}'.format(s_type, s_number+1, i+1)
        if p_type == 'via':
            # PTP
            src_f.write(k_tmpl.ptp_src_template.format(p_name, p_speed))
            dat_f.write(k_tmpl.ptp_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax))
            
        elif p_type in ['s1', 't1', 's2', 't2', 's3v2', 't3']:
            # SEARCH LIN
            # t1 t2 t3 игнорируем, это референсные точки для поиска
            if p_type in ['s1', 's2', 's3v2']:
                p2 = statement[i+1]
                p2_xyz, p2_cba, p2_S, p2_T, p2_speed, p2_type, p2_gantry, p2_ext_ax = p2
                p2_name = '{0}{1}_{2}'.format(s_type, s_number+1, i+2)
                src_f.write(k_tmpl.search_src_template.format(p_name, p_speed, p2_name))
                dat_f.write(k_tmpl.search_dat_template.format(
                    p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax,
                    p2_name, p2_speed, *p2_xyz, *reversed(p2_cba), p2_S, p2_T, *p2_ext_ax,
                    ))
                cd0 = 'TSg_CD0'
                if p_type == 's1':
                    cd1 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1)
                    #src_f.write(';FOLD Corr\nTS_BC6DCalc({0}, {1}, {2}, {3}, {4}, {5})\n;ENDFOLD'.format(cd1, cd0, cd0, cd0, cd0, cd0))
                    src_f.write(k_tmpl.corr_src_template_1d.format(1, cd1, cd0, cd0, cd0, cd0, cd0))
                if p_type == 's2':
                    cd1 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1-2)
                    cd2 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1)
                    #src_f.write(';FOLD Corr\nTS_BC6DCalc({0}, {1}, {2}, {3}, {4}, {5})\n;ENDFOLD'.format(cd1, cd0, cd0, cd2, cd0, cd0))
                    src_f.write(k_tmpl.corr_src_template_2d.format(2, cd1, cd0, cd0, cd2, cd0, cd0))
                elif p_type == 's3v2':
                    cd1 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1-5)
                    cd2 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1-3)
                    cd3 = 'CD{0}{1}_{2}'.format(s_type, s_number+1, i+1)
                    #src_f.write(';FOLD Corr\nTS_BC6DCalc({0}, {1}, {2}, {3}, {4}, {5})\n;ENDFOLD'.format(cd1, cd0, cd0, cd2, cd0, cd3))
                    src_f.write(k_tmpl.corr_src_template_3d.format(3, cd1, cd0, cd0, cd2, cd0, cd3))
        elif p_type == 'arcon':
            # ARCON
            src_f.write('\ncustom_sync(\'h{0:02X}{0:02X}0000\', \'hffff0000\', {0})\n'.format(250))
            src_f.write(k_tmpl.arcon_src_template.format(p_name, p_speed))
            dat_f.write(k_tmpl.arcon_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax, job, velocity/6000))
        elif p_type == 'arcoff':
            # ARCOFF
            src_f.write(k_tmpl.arcoff_src_template.format(p_name, p_speed))
            dat_f.write(k_tmpl.arcoff_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax, job, velocity/6000))
        else:
            # LIN
            src_f.write(k_tmpl.lin_src_template.format(p_name, p_speed))
            dat_f.write(k_tmpl.lin_dat_template.format(p_name, p_speed, *p_xyz, *reversed(p_cba), p_S, p_T, *p_ext_ax))
        

#statements = gen_seq_sample_2()
#statements += gen_seq_sample_1()
#statements = gen_seq_one_seam(1, 'left', 'start-to-middle')

def write_seam_src_dat(edge_num, side, seam_type, folder='d:\\'):
    st = ''.join([w[0] for w in seam_type.split('-')])
    prog_name = 'edge_{0}_{1}_{2}'.format(edge_num, side, st)
    statements = gen_seq_one_seam(edge_num, side, seam_type)
    #with open(folder + prog_name+'.src', 'w') as src_f:
    with open(os.path.join(folder, prog_name+'.src'), 'w') as src_f:
        src_f.write(k_tmpl.src_header_template.format(prog_name))
        #with open(folder + prog_name+'.dat', 'w') as dat_f:
        with open(os.path.join(folder, prog_name+'.dat'), 'w') as dat_f:
            dat_f.write(k_tmpl.dat_header_template.format(prog_name))
            for i, statement_rec in enumerate(statements):
                s_type, ii, statement = statement_rec    
                #write_out_statement(src_f, dat_f, s_type, i, ii, statement)
                write_out_statement(src_f, dat_f, s_type, i, statement)

            dat_f.write(k_tmpl.dat_footer_template)
        src_f.write('\nTS_CorrOff()\n')
        src_f.write('ochistka_mod()\n')
        src_f.write(k_tmpl.src_footer_template)
    return prog_name, statements


def generate(out_dir, main_name, robot_num = 1):
    if not out_dir:
        out_dir = ''
    if not main_name:
        main_name = 'main_gen.src'
    out_dir_rob = os.path.join(out_dir,'R'+str(robot_num))
    shutil.rmtree(out_dir_rob, ignore_errors = True)
    os.makedirs(out_dir_rob)
    edge_seqence = [i for i in range(detail_params['edge_count']) if i % 2] \
                 + [i for i in range(detail_params['edge_count']) if not i % 2]
    main_f = os.path.join(out_dir_rob, main_name)
    with open(main_f, 'w') as f:
        f.write(k_tmpl.src_header_template.format('main_gen'))
        f.write('\nunfold()\n')
        f.write('\ntcptrack_mod()\n')
        f.write('ochistka_mod()\n\n')
        sync = 1
        for side in ['right', 'left']:
            for edge_num in edge_seqence:
                seam_type = 'middle-to-end'
                if robot_num == 1:
                    seam_type = 'start-to-middle'
                
                prog_name, statements = write_seam_src_dat(edge_num, side, seam_type, out_dir_rob)
                f.write('custom_sync(\'h{0:02X}{0:02X}0000\', \'hffff0000\', {0})\n'.format(sync))
                f.write('{0}()\n\n'.format(prog_name))
                sync += 1
        f.write('custom_sync(\'h{0:02X}{0:02X}0000\', \'hffff0000\', {0})\n'.format(sync))
        f.write('\ngo_home()\n')
        f.write('\nfold()\n')
        f.write(k_tmpl.src_footer_template)
        e1_home = -11400
        if robot_num == 1:
            e1_home = -6000
        f.write(k_tmpl.def_go_home_template.format(e1_home))
        

if __name__ == '__main__':
    import argparse, yaml
    import logging
    import sys
    logging.basicConfig(filename='generator.log',    
                        format="[%(asctime)s] %(levelname)s %(message)s",
                        datefmt="%d/%b/%Y %H:%M:%S", level=logging.INFO)
    parser = argparse.ArgumentParser(description="welding KRL generator")
    parser.add_argument("-p", dest="detail_params")
    parser.add_argument("-o", dest="output_dir")
    parser.add_argument("-n", dest="main_name")
    args, leftovers = parser.parse_known_args()
    logging.info('Start generating: ' + ' '.join(sys.argv))
    if args.detail_params:
        #logging.info('Load detail config from ' + args.detail_params)
        with open(args.detail_params, 'r') as f:
            new_params = json.load(f)
            detail_params.update(new_params)
    #print(yaml.dump(detail_params, default_flow_style=False))
    logging.info('detail_params:\n'+yaml.dump(detail_params, default_flow_style=False))
    for r in range(2):
        generate(args.output_dir, args.main_name, r+1)
