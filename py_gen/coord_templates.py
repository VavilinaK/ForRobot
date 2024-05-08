import numpy as np

class CoordTemplates:
    def __init__(self, detail_params):
        self.detail_params = detail_params
    
    #100 right s20 t53
    #100 left s20 t29
    
    #000 right s16 t13
    #000 left s16 t37
    
    # шаблоны для шва 
    # (x,y,z), (c,b,a), S, T
    @property
    def weld_template_left(self):
        h = self.detail_params['h']
        return {
        'via': (np.array([0, -60, h+150]), np.array([-160.3, 41.3, 118.5]), 20, 29),
        'near': (np.array([0, -60, 60]), np.array([-160.3, 41.3, 118.5]), 20, 29),
        'weld': (np.array([0, 0, 0]), np.array([-160.3, 41.3, 118.5]), 20, 29),
        }
    
    @property
    def weld_template_right(self):
        h = self.detail_params['h']
        return {
        'via': (np.array([0, 60, h+150]), np.array([160.3, 41.3, -118.5]), 20, 53),
        'near': (np.array([0, 60, 60]), np.array([160.3, 41.3, -118.5]), 20, 53),
        'weld': (np.array([0, 0, 0]), np.array([160.3, 41.3, -118.5]), 20, 53),
        }


    # шаблоны для поиска
    # (x,y,z), (c,b,a), S, T
    @property
    def search_template_left_start(self):
        h = self.detail_params['h']
        d_s1 = self.detail_params['d_s1']
        l_r1 = self.detail_params['l_r1']
        return {
        'via': (np.array([-d_s1, -60, h+150]), np.array([-180, 50, 120]), 20, 29),
        's1':(np.array([-d_s1, -60, 60]), np.array([-180, 50, 120]), 20, 29),
        't1':(np.array([-d_s1, -60, 0]), np.array([-180, 50, 120]), 20, 29),
        's2':(np.array([-d_s1, -60, 60]), np.array([169, 50, 120]), 20, 29),
        't2':(np.array([-d_s1, 0, 60]), np.array([169, 50, 120]), 20, 29),
        's3v1':(np.array([60 + l_r1, -60, 60]), np.array([-169, 40, 120]), 20, 29),
        's3v2':(np.array([60 + l_r1, 10, 60]), np.array([-169, 40, 120]), 20, 29),
        't3':(np.array([l_r1, 10, 60]), np.array([-169, 40, 120]), 20, 29),
        'departure':(np.array([60 + l_r1, -60, h+100]), np.array([-169, 50, 150]), 20, 29),
        }
    
    @property
    def search_template_left_end(self):
        h = self.detail_params['h']
        d_s2 = self.detail_params['d_s2']
        l_r2 = self.detail_params['l_r2']
        return {
        'via': (np.array([d_s2, -60, h+150]), np.array([-180, 50, 45]), 20, 29),
        's1':(np.array([d_s2, -60, 60]), np.array([-180, 50, 45]), 20, 29),
        't1':(np.array([d_s2, -60, 0]), np.array([-180, 50, 45]), 20, 29),
        's2':(np.array([d_s2, -60, 60]), np.array([169, 50, 60]), 20, 29),
        't2':(np.array([d_s2, 0, 60]), np.array([169, 50, 60]), 20, 29),
        's3v1':(np.array([-60 - l_r2, -60, 60]), np.array([-169, 40, 30]), 20, 29),
        's3v2':(np.array([-60 - l_r2, 10, 60]), np.array([-169, 40, 30]), 20, 29),
        't3':(np.array([-l_r2, 10, 60]), np.array([-169, 40, 30]), 20, 29),
        'departure':(np.array([-60 - l_r2, -60, h+100]), np.array([-169, 50, 30]), 20, 29),
        }

    @property
    def search_template_right_start(self):
        h = self.detail_params['h']
        d_s1 = self.detail_params['d_s1']
        l_r1 = self.detail_params['l_r1']
        return {
        'via': (np.array([-d_s1, 60, h+150]), np.array([-180, 50, -120]), 20, 53),
        's1':(np.array([-d_s1, 60, 60]), np.array([-180, 50, -120]), 20, 53),
        't1':(np.array([-d_s1, 60, 0]), np.array([-180, 50, -120]), 20, 53),
        's2':(np.array([-d_s1, 60, 60]), np.array([169, 50, -120]), 20, 53),
        't2':(np.array([-d_s1, 0, 60]), np.array([169, 50, -120]), 20, 53),
        's3v1':(np.array([60 + l_r1, 60, 60]), np.array([-169, 40, -120]), 20, 53),
        's3v2':(np.array([60 + l_r1, -10, 60]), np.array([-169, 40, -120]), 20, 53),
        't3':(np.array([l_r1, -10, 60]), np.array([-169, 40, -120]), 20, 53),
        'departure':(np.array([60 + l_r1, 60, h+100]), np.array([-169, 50, -150]), 20, 53),
        }

    @property
    def search_template_right_end(self):
        h = self.detail_params['h']
        d_s2 = self.detail_params['d_s2']
        l_r2 = self.detail_params['l_r2']
        return {
        'via': (np.array([d_s2, 60, h+150]), np.array([-180, 50, -45]), 20, 53),
        's1':(np.array([d_s2, 60, 60]), np.array([-180, 50, -45]), 20, 53),
        't1':(np.array([d_s2, 60, 0]), np.array([-180, 50, -45]), 20, 53),
        's2':(np.array([d_s2, 60, 60]), np.array([169, 50, -60]), 20, 53),
        't2':(np.array([d_s2, 0, 60]), np.array([169, 50, -60]), 20, 53),
        's3v1':(np.array([-60 - l_r2, 60, 60]), np.array([-169, 40, -30]), 20, 53),
        's3v2':(np.array([-60 - l_r2, -10, 60]), np.array([-169, 40, -30]), 20, 53),
        't3':(np.array([-l_r2, -10, 60]), np.array([-169, 40, -30]), 20, 53),
        'departure':(np.array([-60 - l_r2, 60, h+100]), np.array([-169, 50, -30]), 20, 53),
    }
