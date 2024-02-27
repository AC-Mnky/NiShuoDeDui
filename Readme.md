# 从来没写过Readme的人的第一次乱写

使用的游戏引擎是.NET 6.0下的MonoGame框架。（严格来说MonoGame算不上游戏引擎，也没有大量复杂的黑盒功能，这使得它的版本不那么重要。）（可以使用最新版本的.NET，反正是向下兼容的。我用的是.NET 8.0.1）

建议使用Visual Studio或VS Code，我自己用的是VS Code（我似乎把配置文件也同步上来了），看的是这个教程 https://learn-monogame.github.io/ 。如果想用Visual Studio的话Monogame官网上应该有教程。

直接命令行`dotnet run`应该就能跑了。记得先`cd TheGame`。

源码的部分就是TheGame文件夹下的那些cs文件啦（cs代表CSharp）。不知道从哪个开始看的话可以先看Game1.cs。

说来C#有一些很烦的地方，比如那些private啊protected啊的修饰词满天飞。反正我们搞的也不是什么安全性要求很高的庞大软件工程，嫌麻烦的话干脆就都改成public得了。

