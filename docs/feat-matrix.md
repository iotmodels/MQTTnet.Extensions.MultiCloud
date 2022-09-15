# Feature Matrix by Broker

The table below shows the diff of available MQTT features by cloud vendor.

| Feature | Mosquitto| Azure IoT Hub | AWS IoTCore | Hive MQ Cloud |
|---------|----------|---------------|-------------|---------|
| Web Sockets| :white_check_mark: | :white_check_mark: | :white_check_mark: | :white_check_mark:|
| Basic Auth | :white_check_mark: | :x: | :x: | :white_check_mark:|
| X509 Auth | :white_check_mark: | :white_check_mark: (with custom username) | :white_check_mark: | :x: |
| Sas Auth| :x: | :white_check_mark: | :x:  | :x: | :x:|
| Custom Topics | :white_check_mark: | :x: | :white_check_mark: | :white_check_mark:|
| Telemetry Routing| :x: | :white_check_mark: | :white_check_mark: | :x: |
| Property Store|  :asterisk: (Retained) | :white_check_mark: (Device Twins) | :white_check_mark: (Device Shadows) |  :asterisk: (Retained)|
| HTTP Command API | :x: | :white_check_mark: | :x: | :x:|

