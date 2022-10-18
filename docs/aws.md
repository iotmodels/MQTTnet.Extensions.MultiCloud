# Connecting to AWS IoT Core

AWS IoT Core requires X509 client certificates to connecto to the MQTT endpoint. 

These certificates can be self-signed or CA signed.

Additionally you might need to create a Thing identity, or use a Provisioning template.

https://docs.aws.amazon.com/iot/latest/developerguide/single-thing-provisioning.html

https://docs.aws.amazon.com/iot/latest/developerguide/auto-register-device-cert.html

https://aws.amazon.com/blogs/iot/just-in-time-registration-of-device-certificates-on-aws-iot/

As with any other MQTT broker, you can use WithConnectionSettings including the X509Key

To support JIT, the first connection always fails and a retry is needed, in that case you can use the AwsClientFactory


## Shadows

To use shadows, you must configure a "thing", associate to a ceritifcate, and enable a classic shadow.

