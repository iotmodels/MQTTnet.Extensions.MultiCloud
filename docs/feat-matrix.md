# Feature Matrix by Broker

| Feature | Mosquitto| Azure IoT Hub | AWS IoTCore | Hive MQ Cloud |
|---------|----------|---------------|-------------|---------|
| Basic Auth | :white_check_mark: | :x: | :x: | :white_check_mark:|
| X509 Auth | :white_check_mark: | :white_check_mark: | :white_check_mark: | :x: |
| Sas Auth| :x: | :x: | :white_check_mark: | :x: | :x:|
| Custom Topics | :white_check_mark: | :x: | :white_check_mark: | :white_check_mark:|
| Telemetry Routing| :x: | :white_check_mark: | :white_check_mark: | :x: |
| Property Store|  :asterisk: (Retained) | :white_check_mark: (Device Twins) | :white_check_mark: (Device Shadows) |  :asterisk: (Retained)|
| HTTP Command API | :x: | :white_check_mark: | :x: | :x:|

