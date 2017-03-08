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

Level colors

 - Trace ![trace](https://s30.postimg.org/98xatnf5d/trace.png)
 - Debug ![debug](https://s27.postimg.org/tqrzxdb5v/debug.png)
 - Info ![info](https://s28.postimg.org/mvrf8an7h/info.png)
 - Error ![error](https://s28.postimg.org/uelelq6r1/error.png)
 - Fatal ![fatal](https://s10.postimg.org/kj618178p/fatal.png)



![hipchat](https://s16.postimg.org/608xjfv5x/hipchat.png)

![hipchat2](https://s1.postimg.org/h46spj2db/hipchat2.png)
----------

