NLog Hipchat
-------------
Is a custom target for Hipchat

        <target xsi:type="HipChat"
                name="h"
                layout="${uppercase:${level}} | ${message} | ${exception} | ${stacktrace:format=Flat} | ${appdomain}"
                token="your-token"
                roomid="your-room-id"
                site="your-website-url"
                icon="icon-for-card"
                host="your-host"
                              />
Don't forget to add your rule

    <logger name="*" minlevel="Debug" writeTo="h" />

![hipchat](https://s16.postimg.org/608xjfv5x/hipchat.png)
----------

