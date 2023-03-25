
import cv2
import time
import math
import numpy as np
import mediapipe as mp
 
# mediapipe配置
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_hands = mp.solutions.hands
hands =  mp_hands.Hands(
    static_image_mode=True,
    max_num_hands=2,
    min_detection_confidence=0.5)
 
 
# 调用摄像头 0 默认摄像头 
cap = cv2.VideoCapture(0)
 
# cv2.namedWindow("frame", 0)
# cv2.resizeWindow("frame", 960, 640)
 
 
# 获取画面宽度、高度
width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
 
 
# 方块初始数组
x = 100
y = 100
w = 100
h = 100
 
L1 = 0
L2 = 0
 
on_square = False
square_color = (0, 255, 0)
 
# 读取一帧帧照片
while True:
    # 返回frame图片
    rec,frame = cap.read()
    
    # 镜像
    frame = cv2.flip(frame,1)
    
    
    
    frame.flags.writeable = False
    frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    # 返回结果
    results = hands.process(frame)
 
    frame.flags.writeable = True
    frame = cv2.cvtColor(frame, cv2.COLOR_RGB2BGR)
    
    
    # 如果结果不为空
    if results.multi_hand_landmarks:
 
 
        # 遍历双手(根据读取顺序，一只只手遍历、画画)
        # results.multi_hand_landmarks n双手
        # hand_landmarks 每只手上21个点信息
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(
                frame,
                hand_landmarks,
                mp_hands.HAND_CONNECTIONS,
                mp_drawing_styles.get_default_hand_landmarks_style(),
                mp_drawing_styles.get_default_hand_connections_style())
            
            # 记录手指每个点的x y 坐标
            x_list = []
            y_list = []
            for landmark in hand_landmarks.landmark:
                x_list.append(landmark.x)
                y_list.append(landmark.y)
                
            
            # 获取食指指尖
            index_finger_x, index_finger_y = int(x_list[8] * width),int(y_list[8] * height)
 
            # 获取中指
            middle_finger_x,middle_finger_y = int(x_list[12] * width), int(y_list[12] * height)
 
 
            # 计算两指尖距离
            finger_distance = math.hypot((middle_finger_x - index_finger_x), (middle_finger_y - index_finger_y))
 
            # 如果双指合并(两之间距离近)
            if finger_distance < 60:
 
                # X坐标范围 Y坐标范围
                if (index_finger_x > x and index_finger_x < (x + w)) and (
                        index_finger_y > y and index_finger_y < (y + h)):
 
                    if on_square == False:
                        L1 = index_finger_x - x
                        L2 = index_finger_y - y
                        square_color = (255, 0, 255)
                        on_square = True
 
            else:
                # 双指不合并/分开
                on_square = False
                square_color = (0, 255, 0)
 
            # 更新坐标
            if on_square:
                x = index_finger_x - L1
                y = index_finger_y - L2
            
            
 
    # 图像融合 使方块不遮挡视频图片
    overlay = frame.copy()
    cv2.rectangle(frame, (x, y), (x + w, y + h), square_color, -1)
    frame = cv2.addWeighted(overlay, 0.5, frame, 1 - 0.5, 0)
    
 
    # 显示画面
    cv2.imshow('frame',frame)
    
    # 退出条件
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    
cap.release()
cv2.destroyAllWindows()