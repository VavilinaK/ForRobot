#======= HEADER ========
#------- SRC -----------
src_header_template = '''&ACCESS RVP
&REL 45
&PARAM EDITMASK = *
&PARAM DISKPATH = KRC:\R1\Program
DEF {0}( )
;FOLD INI;%{{PE}}
  ;FOLD BASISTECH INI
    GLOBAL INTERRUPT DECL 3 WHEN $STOPMESS==TRUE DO IR_STOPM ( )
    INTERRUPT ON 3 
    BAS (#INITMOV,0 )
  ;ENDFOLD (BASISTECH INI)
  ;FOLD USER INI
    ;Make your modifications here

  ;ENDFOLD (USER INI)
;ENDFOLD (INI)
'''
#------- DAT -----------
dat_header_template = '''&ACCESS RVP
&REL 45
&PARAM EDITMASK = *
&PARAM DISKPATH = KRC:\R1\Program
DEFDAT  {0}
;FOLD EXTERNAL DECLARATIONS;%{{PE}}%MKUKATPBASIS,%CEXT,%VCOMMON,%P
;FOLD BASISTECH EXT;%{{PE}}%MKUKATPBASIS,%CEXT,%VEXT,%P
EXT  BAS (BAS_COMMAND  :IN,REAL  :IN )
DECL INT SUCCESS
;ENDFOLD (BASISTECH EXT)
;FOLD USER EXT;%{{E}}%MKUKATPUSER,%CEXT,%VEXT,%P
;Make your modifications here

;ENDFOLD (USER EXT)
;ENDFOLD (EXTERNAL DECLARATIONS)
'''


#======= FOOTER ========
#------- SRC -----------
src_footer_template = '''END
'''
#------- DAT -----------
dat_footer_template = '''ENDDAT
'''

#======= PTP ===========
#------- SRC -----------
#;FOLD PTP W10_1 Vel= 100.0 % PDATW10_1 Tool[1] Base[10] ;%{PE}%R 8.3.34,%MKUKATPBASIS,%CMOVE,%VPTP,%P 1:PTP, 2:W10_1, 3:C_DIS, 5:100.0, 7:PDATW10_1
#$BWDSTART = FALSE
#PDAT_ACT=PPDATW10_1
#FDAT_ACT=FW10_1
#BAS(#PTP_PARAMS,100.0)
#SET_CD_PARAMS (0)
#PTP XW10_1 C_DIS
#;ENDFOLD

ptp_src_template = '''
;FOLD PTP {0} Vel= {1} % PDATW1_1 Tool[1] Base[10] ;%{{PE}}%R 8.3.34,%MKUKATPBASIS,%CMOVE,%VPTP,%P 1:PTP, 2:{0}, 3:C_DIS, 5:{1}, 7:PDAT{0}
$BWDSTART = FALSE
PDAT_ACT=PPDAT{0}
FDAT_ACT=F{0}
BAS(#PTP_PARAMS,{1})
SET_CD_PARAMS (0)
PTP X{0} C_DIS
;ENDFOLD
'''
#------- DAT -----------
#DECL PDAT PPDATW10_1={VEL 100.0,ACC 100.0,APO_DIST 10.0,GEAR_JERK 50.0000,EXAX_IGN 0}
#DECL FDAT FW10_1={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}
#DECL E6POS XW10_1={X 3681.051,Y 1163.673,Z 466.21,A 118.496,B 41.311,C -160.284,S 16,T 37,E1 -2894.079,E2 0.0,E3 294.263,E4 0.0,E5 0.0,E6 0.0}

ptp_dat_template = '''
DECL PDAT PPDAT{0}={{VEL {1},ACC 100.0,APO_DIST 10.0,GEAR_JERK 50.0000,EXAX_IGN 0}}
DECL FDAT F{0}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}}
DECL E6POS X{0}={{X {2},Y {3},Z {4},A {5},B {6},C {7},S {8},T {9},E1 {10},E2 {11},E3 {12},E4 0,E5 0,E6 0}}
'''


#======= LIN ========
#------- SRC -----------
#;FOLD LIN S1_2 Vel= 1.0 m/s CPDATS1_2 Tool[1] Base[10] ;%{PE}%R 8.3.34,%MKUKATPBASIS,%CMOVE,%VLIN,%P 1:LIN, 2:S1_2, 3:, 5:1.0, 7:CPDATS1_2
#$BWDSTART = FALSE
#LDAT_ACT=LCPDATS1_2
#FDAT_ACT=FS1_2
#BAS(#CP_PARAMS,1.0)
#SET_CD_PARAMS (0)
#LIN XS1_2 C_DIS C_DIS
#;ENDFOLD
lin_src_template = '''
;FOLD LIN {0} Vel= {1} m/s CPDATS1_2 Tool[1] Base[10] ;%{{PE}}%R 8.3.34,%MKUKATPBASIS,%CMOVE,%VLIN,%P 1:LIN, 2:{0}, 3:, 5:{1}, 7:CPDAT{0}
$BWDSTART = FALSE
LDAT_ACT=LCPDAT{0}
FDAT_ACT=F{0}
BAS(#CP_PARAMS,{1})
SET_CD_PARAMS (0)
LIN X{0} C_DIS C_DIS
;ENDFOLD
'''

#------- DAT -----------
#DECL E6POS XS1_2={X 1625.693,Y -6197.439,Z 1071.301,A 164.694,B 49.055,C 174.201,S 22,T 27,E1 -6945.305,E2 621.707,E3 -549.8,E4 0.0,E5 0.0,E6 0.0}
#DECL FDAT FS1_2={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}
#DECL LDAT LCPDATS1_2={VEL 1.0,ACC 100.0,APO_DIST 10.0,APO_FAC 50.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0,GEAR_JERK 50.0000,EXAX_IGN 0}
lin_dat_template = '''
DECL LDAT LCPDAT{0}={{VEL {1},ACC 100.0,APO_DIST 10.0,APO_FAC 50.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0,GEAR_JERK 50.0000,EXAX_IGN 0}}
DECL FDAT F{0}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}}
DECL E6POS X{0}={{X {2},Y {3},Z {4},A {5},B {6},C {7},S {8},T {9},E1 {10},E2 {11},E3 {12},E4 0,E5 0,E6 0}}
'''

#======= SEARCH ========
#------- SRC -----------
#;FOLD LIN S1_4 Vel= 0.06 m/s CPDATS1_4 TouchSense SEARCH VIA S1_5 CD1 SP1 Tool[1] Base[10] ;%{PE}
#;FOLD Parameters ;%{h}
#;Params IlfProvider=kukaroboter.touchsense.searchlin; Kuka.PointName=S1_4; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDATS1_4; Kuka.VelocityPath=0.06; Kuka.MovementParameterFieldEnabled=True; MoveType=LIN
#;FOLD Parameters TouchSense ;%{h}
#;Params TouchSense.SearchViaPoint=S1_5;TouchSense.ReferenceMove=CD1;TouchSense.SeachParam=SP1
#;ENDFOLD
#;ENDFOLD
#$BWDSTART = FALSE
#LDAT_ACT=LCPDATS1_4
#FDAT_ACT=FS1_4
#BAS(#CP_PARAMS,0.06)
#SET_CD_PARAMS (0)
#LIN XS1_4 C_DIS C_DIS
#TS_InitMovement($POS_ACT, XS1_5, ZSP1)
#TS_ManMoveToVia(XS1_5)
#TS_Search(XS1_5, ZSP1, VCD1)
#TS_SetCorrData(ZSP1, VCD1)
#;ENDFOLD
search_src_template = '''
;FOLD LIN {0} Vel= {1} m/s CPDAT{0} TouchSense SEARCH VIA {2} CD{0} SP{0} Tool[1] Base[10] ;%{{PE}}
;FOLD Parameters ;%{{h}}
;Params IlfProvider=kukaroboter.touchsense.searchlin; Kuka.PointName={0}; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDAT{0}; Kuka.VelocityPath={1}; Kuka.MovementParameterFieldEnabled=True; MoveType=LIN
;FOLD Parameters TouchSense ;%{{h}}
;Params TouchSense.SearchViaPoint={2};TouchSense.ReferenceMove=CD{0};TouchSense.SeachParam=SP{0}
;ENDFOLD
;ENDFOLD
$BWDSTART = FALSE
LDAT_ACT=LCPDAT{0}
FDAT_ACT=F{0}
BAS(#CP_PARAMS,{1})
SET_CD_PARAMS (0)
LIN X{0} C_DIS C_DIS
TS_InitMovement($POS_ACT, X{2}, ZSP{0})
TS_ManMoveToVia(X{2})
TS_Search(X{2}, ZSP{0}, VCD{0})
TS_SetCorrData(ZSP{0}, VCD{0})
;ENDFOLD
'''


#------- DAT -----------
#DECL E6POS XS1_4={X 1530.881,Y -6171.491,Z 958.0,A 164.694,B 49.055,C 174.201,S 22,T 27,E1 -6919.357,E2 508.406,E3 -549.8,E4 0.0,E5 0.0,E6 0.0}
#DECL FDAT FS1_4={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " ",TQ_STATE FALSE}
#DECL LDAT LCPDATS1_4={VEL 0.06,ACC 100.0,APO_DIST 10.0,APO_FAC 50.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0}
#DECL E6POS XS1_5={X 1530.881,Y -6171.491,Z 908.0,A 164.694,B 49.055,C 174.201,S 22,T 27,E1 -6919.357,E2 458.406,E3 -549.8,E4 0.0,E5 0.0,E6 0.0}
#DECL FDAT FS1_5={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " ",TQ_STATE FALSE}
#DECL TSg_PCollCD_T VCD1={CD_IPO_MODE #BASE,Mastered TRUE,SDir_W {X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0},Ref_W {X 1530.881,Y -6171.491,Z 908.0,A 164.694,B 49.055,C 174.201},Meas_W {X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0},Diff_W {X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0},VecLenDiff_W 0.0,MEnum #CDEmpty,MC 1}
#DECL TSg_SearchProperty_T ZSP1={MoveType #MoveWithLin,TouchType #Single,DynamicStep #Slow,SensorNum 1,SearchDepth_Tol 150.0,bBackToStartPos TRUE}
search_dat_template = '''
DECL E6POS X{0}={{X {2},Y {3},Z {4},A {5},B {6},C {7},S {8},T {9},E1 {10},E2 {11},E3 {12},E4 0,E5 0,E6 0}}
DECL FDAT F{0}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " ",TQ_STATE FALSE}}
DECL LDAT LCPDAT{0}={{VEL {1},ACC 100.0,APO_DIST 10.0,APO_FAC 50.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0}}
DECL E6POS X{13}={{X {15},Y {16},Z {17},A {18},B {19},C {20},S {21},T {22},E1 {23},E2 {24},E3 {25},E4 0,E5 0,E6 0}}
DECL FDAT F{13}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " ",TQ_STATE FALSE}}
DECL TSg_PCollCD_T VCD{0}={{CD_IPO_MODE #BASE,Mastered TRUE,SDir_W {{X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0}},Ref_W {{X {15},Y {16},Z {17},A {18},B {19},C {20}}},Meas_W {{X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0}},Diff_W {{X 0.0,Y 0.0,Z 0.0,A 0.0,B 0.0,C 0.0}},VecLenDiff_W 0.0,MEnum #CDEmpty,MC 1}}
DECL TSg_SearchProperty_T ZSP{0}={{MoveType #MoveWithLin,TouchType #Single,DynamicStep #Slow,SensorNum 1,SearchDepth_Tol 150.0,bBackToStartPos TRUE}}
'''

#======= CORR ========
#------- SRC -----------

#;FOLD TouchSense Corr 1 CD1;%{E}
#;FOLD Parameters Corr 1D ;%{h}
#;Params IlfProvider=KukaRoboter.TouchSense.Correction1D;Correction1D=CD1
#;ENDFOLD
#TS_BC6DCalc(VCD1, TSg_CD0, TSg_CD0, TSg_CD0, TSg_CD0, TSg_CD0)
#;ENDFOLD

#;FOLD TouchSense Corr 3 CD1 CD2 CD3;%{E}
#;FOLD Parameters Corr 3D ;%{h}
#;Params IlfProvider=KukaRoboter.TouchSense.Correction3D;Correction1D=CD1;Correction2D=CD2;Correction3D=CD3
#;ENDFOLD
#TS_BC6DCalc(VCD1, TSg_CD0, TSg_CD0, VCD2, TSg_CD0, VCD3)
#;ENDFOLD

corr_src_template_1d = '''
;FOLD TouchSense Corr {0} {1};%{{E}}
;FOLD Parameters Corr {0}D ;%{{h}}
;Params IlfProvider=KukaRoboter.TouchSense.Correction{0}D;Correction1D={1}
;ENDFOLD
TS_BC6DCalc(V{1}, {2}, {3}, {4}, {5}, {6})
;ENDFOLD
'''

corr_src_template_2d = '''
;FOLD TouchSense Corr {0} {1} {4};%{{E}}
;FOLD Parameters Corr {0}D ;%{{h}}
;Params IlfProvider=KukaRoboter.TouchSense.Correction{0}D;Correction1D={1};Correction2D={4}
;ENDFOLD
TS_BC6DCalc(V{1}, {2}, {3}, V{4}, {5}, {6})
;ENDFOLD
'''

corr_src_template_3d = '''
;FOLD TouchSense Corr {0} {1} {4} {6};%{{E}}
;FOLD Parameters Corr {0}D ;%{{h}}
;Params IlfProvider=KukaRoboter.TouchSense.Correction{0}D;Correction1D={1};Correction2D={4};Correction3D={6}
;ENDFOLD
TS_BC6DCalc(V{1}, {2}, {3}, V{4}, {5}, V{6})
;ENDFOLD
'''

#======= ARCON ========
#------- SRC -----------
#;FOLD ARCON WDAT1 LIN W8_4 Vel= 0.1 m/s CPDATW8_4 Tool[1] Base[10];%{PE}
#;FOLD Parameters ;%{h}
#;Params IlfProvider=kukaroboter.arctech.arconlin; Kuka.PointName=W8_4; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDATW8_4; Kuka.VelocityPath=0.1; Kuka.CurrentCDSetIndex=0; Kuka.MovementParameterFieldEnabled=True; ArcTech.WdatVarName=WDAT1
#;ENDFOLD
#$BWDSTART = FALSE
#LDAT_ACT = LCPDATW8_4
#FDAT_ACT = FW8_4
#BAS(#CP_PARAMS,0.1)
#SET_CD_PARAMS (0)
#TRIGGER WHEN DISTANCE = 1 DELAY = ArcGetDelay(#PreDefinition, WDAT1) DO ArcMainNG(#PreDefinition, WDAT1, WW8_4) PRIO = -1
#TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOn, WDAT1) DELAY = ArcGetDelay(#GasPreflow, WDAT1) DO ArcMainNG(#GasPreflow, WDAT1, WW8_4) PRIO = -1
#TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOn, WDAT1) DELAY = 0 DO ArcMainNG(#ArcOnMoveStd, WDAT1, WW8_4) PRIO = -1
#ArcMainNG(#ArcOnBeforeMoveStd, WDAT1, WW8_4)
#LIN XW8_4
#ArcMainNG(#ArcOnAfterMoveStd, WDAT1, WW8_4)
#;ENDFOLD
arcon_src_template = '''
;FOLD ARCON WDAT{0} LIN {0} Vel= {1} m/s CPDATW8_4 Tool[1] Base[10];%{{PE}}
;FOLD Parameters ;%{{h}}
;Params IlfProvider=kukaroboter.arctech.arconlin; Kuka.PointName={0}; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDAT{0}; Kuka.VelocityPath={1}; Kuka.CurrentCDSetIndex=0; Kuka.MovementParameterFieldEnabled=True; ArcTech.WdatVarName=WDAT{0}
;ENDFOLD
$BWDSTART = FALSE
LDAT_ACT = LCPDAT{0}
FDAT_ACT = F{0}
BAS(#CP_PARAMS,{1})
SET_CD_PARAMS (0)
TRIGGER WHEN DISTANCE = 1 DELAY = ArcGetDelay(#PreDefinition, WDAT{0}) DO ArcMainNG(#PreDefinition, WDAT{0}, W{0}) PRIO = -1
TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOn, WDAT{0}) DELAY = ArcGetDelay(#GasPreflow, WDAT{0}) DO ArcMainNG(#GasPreflow, WDAT{0}, W{0}) PRIO = -1
TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOn, WDAT{0}) DELAY = 0 DO ArcMainNG(#ArcOnMoveStd, WDAT{0}, W{0}) PRIO = -1
ArcMainNG(#ArcOnBeforeMoveStd, WDAT{0}, W{0})
LIN X{0}
ArcMainNG(#ArcOnAfterMoveStd, WDAT{0}, W{0})
;ENDFOLD
'''

#------- DAT -----------
#DECL stArcDat_T WDAT1={Strike {JobModeId[] "Job Mode",ParamSetId[] "Set1",StartTime 0.0,PreFlowTime 0.0,Channel1 1,Channel2 0.0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0,PurgeTime 0.0},Weld {JobModeId[] "Job Mode",ParamSetId[] "Set2",Velocity 0.0063,Channel1 0,Channel2 0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0},Weave {Pattern #Triangle,Length 3.5,Amplitude 2,Angle 0.0,LeftSideDelay 0.0,RightSideDelay 0.0},Advanced {IgnitionErrorStrategy 1,WeldErrorStrategy 1,SlopeOption #None,SlopeTime 0.0,SlopeDistance 0.0,OnTheFlyActiveOn FALSE,OnTheFlyActiveOff FALSE,OnTheFlyDistanceOn 0.0,OnTheFlyDistanceOff 0.0},Track {IsArcSenseEnabled TRUE,ControlEnabled TRUE,KeepOffset TRUE,TeachNew FALSE,FindSeamMiddle TRUE,TrackingDelay 0.2,ControlSensibility 25.0,MaxDeviation 100,ControlSensibilityHeight 25.0,Bias 0.0,WeaveOffset 0.0,HeightCorrValue 0.0}}
#DECL stArcDat_T WW8_4={Strike {SeamName[] " ",SeamNumber -1},Advanced {BitCodedRobotMark 0}}
#DECL E6POS XW8_4={X 1977.381,Y -6034.5,Z 910.829,A 27.236,B 41.641,C -156.118,S 22,T 59,E1 -6904.319,E2 427.289,E3 -549.8,E4 0.0,E5 0.0,E6 0.0}
#DECL FDAT FW8_4={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}
#DECL LDAT LCPDATW8_4={VEL 0.1,ACC 100.000,APO_DIST 0.0,APO_FAC 50.0000,AXIS_VEL 100.000,AXIS_ACC 100.000,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0000,GEAR_JERK 50.0000,EXAX_IGN 0}
arcon_dat_template = '''
DECL stArcDat_T WDAT{0}={{Strike {{JobModeId[] "Job Mode",ParamSetId[] "Set1",StartTime 0.0,PreFlowTime 0.0,Channel1 {13},Channel2 0.0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0,PurgeTime 0.0}},Weld {{JobModeId[] "Job Mode",ParamSetId[] "Set2",Velocity {14:.4f},Channel1 {13},Channel2 0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0}},Weave {{Pattern #Triangle,Length 3.5,Amplitude 2,Angle 0.0,LeftSideDelay 0.0,RightSideDelay 0.0}},Advanced {{IgnitionErrorStrategy 1,WeldErrorStrategy 1,SlopeOption #None,SlopeTime 0.0,SlopeDistance 0.0,OnTheFlyActiveOn FALSE,OnTheFlyActiveOff FALSE,OnTheFlyDistanceOn 0.0,OnTheFlyDistanceOff 0.0}},Track {{IsArcSenseEnabled TRUE,ControlEnabled TRUE,KeepOffset TRUE,TeachNew FALSE,FindSeamMiddle TRUE,TrackingDelay 0.2,ControlSensibility 25.0,MaxDeviation 100,ControlSensibilityHeight 25.0,Bias 0.0,WeaveOffset 0.0,HeightCorrValue 0.0}}}}
DECL stArcDat_T W{0}={{Strike {{SeamName[] " ",SeamNumber -1}},Advanced {{BitCodedRobotMark 0}}}}
DECL E6POS X{0}={{X {2},Y {3},Z {4},A {5},B {6},C {7},S {8},T {9},E1 {10},E2 {11},E3 {12},E4 0,E5 0,E6 0}}
DECL FDAT F{0}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}}
DECL LDAT LCPDAT{0}={{VEL {1},ACC 100.0,APO_DIST 0.0,APO_FAC 50.0,AXIS_VEL 100.0,AXIS_ACC 100.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0,GEAR_JERK 50.0,EXAX_IGN 0}}
'''


#======= ARCOFF ========
#------- SRC -----------
#;FOLD ARCOFF WDAT3 LIN W8_6 CPDATW8_6 Tool[1] Base[10];%{PE}
#;FOLD Parameters ;%{h}
#;Params IlfProvider=kukaroboter.arctech.arcofflin; Kuka.IsGlobalPoint=False; Kuka.PointName=W8_6; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDATW8_6; Kuka.VelocityPath=0.0063; Kuka.CurrentCDSetIndex=0; Kuka.MovementParameterFieldEnabled=True; ArcTech.WdatVarName=WDAT3
#;ENDFOLD
#$BWDSTART = FALSE
#LDAT_ACT = LCPDATW8_6
#FDAT_ACT = FW8_6
#BAS(#CP_PARAMS, gArcBasVelDefinition)
#SET_CD_PARAMS (0)
#TRIGGER WHEN PATH = ArcGetPath(#ArcOffBefore, WDAT3) DELAY = 0 DO ArcMainNG(#ArcOffBeforeOffStd, WDAT3) PRIO = -1
#TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOff, WDAT3) DELAY = 0 DO ArcMainNG(#ArcOffMoveStd, WDAT3) PRIO = -1
#ArcMainNG(#ArcOffBeforeMoveStd, WDAT3)
#LIN XW8_6
#ArcMainNG(#ArcOffAfterMoveStd, WDAT3)
#;ENDFOLD
arcoff_src_template = '''
;FOLD ARCOFF WDAT{0} LIN {0} CPDAT{0} Tool[1] Base[10];%{{PE}}
;FOLD Parameters ;%{{h}}
;Params IlfProvider=kukaroboter.arctech.arcofflin; Kuka.IsGlobalPoint=False; Kuka.PointName={0}; Kuka.BlendingEnabled=False; Kuka.MoveDataName=CPDAT{0}; Kuka.VelocityPath=0.0063; Kuka.CurrentCDSetIndex=0; Kuka.MovementParameterFieldEnabled=True; ArcTech.WdatVarName=WDAT{0}
;ENDFOLD
$BWDSTART = FALSE
LDAT_ACT = LCPDAT{0}
FDAT_ACT = F{0}
BAS(#CP_PARAMS, gArcBasVelDefinition)
SET_CD_PARAMS (0)
TRIGGER WHEN PATH = ArcGetPath(#ArcOffBefore, WDAT{0}) DELAY = 0 DO ArcMainNG(#ArcOffBeforeOffStd, WDAT{0}) PRIO = -1
TRIGGER WHEN PATH = ArcGetPath(#OnTheFlyArcOff, WDAT{0}) DELAY = 0 DO ArcMainNG(#ArcOffMoveStd, WDAT{0}) PRIO = -1
ArcMainNG(#ArcOffBeforeMoveStd, WDAT{0})
LIN X{0}
ArcMainNG(#ArcOffAfterMoveStd, WDAT{0})
;ENDFOLD
'''

#------- DAT -----------
#DECL stArcDat_T WDAT3={Crater {JobModeId[] "Job Mode",ParamSetId[] "Set3",CraterTime 0.0,PostflowTime 0.0,Channel1 1,Channel2 0.0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0,BurnBackTime 0.0},Advanced {IgnitionErrorStrategy 1,WeldErrorStrategy 1,SlopeOption #None,SlopeTime 0.0,SlopeDistance 0.0,OnTheFlyActiveOn FALSE,OnTheFlyActiveOff FALSE,OnTheFlyDistanceOn 0.0,OnTheFlyDistanceOff 0.0}}
#DECL E6POS XW8_6={X 1977.382,Y -5679.5,Z 910.829,A 47.853,B 33.863,C -165.0,S 22,T 59,E1 -6601.513,E2 406.183,E3 -549.8,E4 0.0,E5 0.0,E6 0.0}
#DECL FDAT FW8_6={TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}
#DECL LDAT LCPDATW8_6={VEL 0.0063,ACC 100.000,APO_DIST 0.0,APO_FAC 50.0000,AXIS_VEL 100.000,AXIS_ACC 100.000,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0000,GEAR_JERK 50.0000,EXAX_IGN 0}
arcoff_dat_template = '''
DECL stArcDat_T WDAT{0}={{Crater {{JobModeId[] "Job Mode",ParamSetId[] "Set3",CraterTime 0.0,PostflowTime 0.0,Channel1 {13},Channel2 0.0,Channel3 0.0,Channel4 0.0,Channel5 0.0,Channel6 0.0,Channel7 0.0,Channel8 0.0,BurnBackTime 0.0}},Advanced {{IgnitionErrorStrategy 1,WeldErrorStrategy 1,SlopeOption #None,SlopeTime 0.0,SlopeDistance 0.0,OnTheFlyActiveOn FALSE,OnTheFlyActiveOff FALSE,OnTheFlyDistanceOn 0.0,OnTheFlyDistanceOff 0.0}}}}
DECL E6POS X{0}={{X {2},Y {3},Z {4},A {5},B {6},C {7},S {8},T {9},E1 {10},E2 {11},E3 {12},E4 0,E5 0,E6 0}}
DECL FDAT F{0}={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}}
DECL LDAT LCPDAT{0}={{VEL {1},ACC 100.0,APO_DIST 0.0,APO_FAC 50.0,AXIS_VEL 100.0,AXIS_ACC 100.0,ORI_TYP #VAR,CIRC_TYP #BASE,JERK_FAC 50.0,GEAR_JERK 50.0,EXAX_IGN 0}}
'''

def_go_home_template = '''

DEF go_home()
;FOLD HOME
DECL E6AXIS XPHOME
XPHOME = {{A1 0.0,A2 0.0,A3 -130.0,A4 0.0,A5 -50.0,A6 -90.0,E1 {0},E2 0.0,E3 -350.0,E4 0.0,E5 0.0,E6 0.0}}
ptp $axis_act
XPHOME.E2 = $axis_act.E2
PDAT_ACT={{VEL 100.0,ACC 100.0,APO_DIST 10.0,GEAR_JERK 50.0000,EXAX_IGN 0}}
FDAT_ACT={{TOOL_NO 1,BASE_NO 10,IPO_FRAME #BASE,POINT2[] " "}}
BAS(#PTP_PARAMS,100.0)
SET_CD_PARAMS (0)
PTP XPHOME C_DIS

;ENDFOLD
END
'''
