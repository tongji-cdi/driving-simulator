# MR Drive Simulator
## 功能
- 驾驶模拟
- VR接入和 hand tracking 支持
- mr touch screen 支持
## 项目文件构成
本工程文件基于实验室 MR Platform 模板项目进行制作。模板项目文件包含了VR支持、光捕系统支持、MR触摸屏支持等功能，但本项目仅使用了VR支持和MR触摸屏支持的功能，具体见Assets里的MR Platform文件夹，另外AddressableAssetsData,Oculus,OptiTrack,XR,WebSocketServer等也是模板的一部分。

其他依赖如下：
- [ NWH Vehicle Physics 2 ](http://nwhvehiclephysics.com/doku.php/Setup/QuickStart/) : 提供了汽车模拟、外设接入的支持、场景模型
- Logitech SDK：斯图马特外设的支持

## 场景说明
场景所在目录：Assets/DriveSim/Scenes/MRDrivingScene

场景内容：
- SceneManager: 外设输入支持，具体见 `SteeringWheelInputProvider` 脚本。
- DemoUICanvas：NWH插件自带，用来在屏幕上显示汽车的输入状态、速度、帧数等信息。
- SportCar：汽车模型以及控制设备。OVRCameraRig（VR组件） 和 MRScreen（MR触摸屏UI组件） 挂在该对象下面
- Environment： NWH自带道路场景模型文件
- LapTime：NWH自带，功能未知，开启和关闭应该不会影响运行
- MR Sceen Manager：用来接受用户屏幕输入数据（坐标和事件）和发送该数据至MRScreen。其中包含了子对象WebSocketServer组件，用来创建WebSocket服务器端和接受用户触摸屏交互数据
- MR EventSystem：里面的MRInputModule 被魔改过，同时支持MR触摸屏的事件处理和VR交互事件的处理。

## 运行场景
1. 开启Oculus Link模式。开启电脑上的Oculis客户端，将头显通过Link模式连接至电脑。
2. 校对原点坐标。需要确保Oculus的原点坐标位置是在驾驶位置上，否则运行场景后会偏移（也可以在运行时校准）
3. 在电脑上运行[mr-touchscreen](https://github.com/shaunabanana/mr-touchscreen)的Web服务。可以通过[Chrome APP :200 OK Webserver](chrome://apps/) 来运行：
4. 触摸屏上通过浏览器打开mr touchscreen 的地址，输入运行unity项目电脑的ip，端口默认5555，点击连接（未运行unity场景的时候会一直连接，运行unity场景后会自动连接）
5. 启动场景。

## 可能遇到的问题
### Oculus Link 连不上
可能是线的问题。咨询葛亚特

### MR触摸屏功能失效。
- 确保在同一Wifi下面
- 在平板上重新连接
- 确保场景中MRScreen的画布分辨率和触摸屏的分辨率相同

### 外设输入无效
这里情况会比较复杂。因为对斯图马特方向盘的API支持是通过罗技的SDK支持,Dell笔记本是安装好了基础环境，如果换电脑了要重新安装 Logitech Gaming Software。

有了基础环境之后，可以通过下面流程来Dubug：
1. 确保外设有信息输入。打开工程文件夹下面 的`LogitechSteeringWheelSDK_8.75.30\Demo\SteeringWheelSDKDemo`， 查看是否能获取到外设的信息输入，以及方向盘、油门、刹车、离合器的输入信号
2. 根据上面一步所查询到的输入信号类型，在场景中的`SceneManager`对象下面的`SteeringWheelInputProvider` 更改axis的输入信息。
3. 以及打开相同对象下面的`Logitech Steering WHeel` 脚本，运行场景查看输入信号是否正确


## 待完善内容
### MR
- 目前虚拟方向盘、座椅、HMI屏幕都没有和实际物体所匹配，需要修改虚拟模型和调整实际物体位置。
- MR TouchScreen 未支持多点触控和滑动交互。
- 现在的TouchScreen页面还未支持在安卓平板上全屏显示，需要优化代码，支持在安卓平板上全屏显示。

### 驾驶模拟
- 外设驾驶控制的一些参数需要调整，现在速度太快了以及反向盘的力反馈很不自然
