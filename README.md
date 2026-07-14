# 一个跨平台的简易双(多)时钟, 支持全屏和置顶小窗、模拟表盘显示，支持快捷键切换、开机自启，支持linux和windows系统。
# A simple cross-platform dual (multi) clock, supporting full-screen and top-pinned small windows, analog watch face display, shortcut key switching, startup on boot, and compatible with Linux and Windows systems.

- 全屏显示 Full-screen display
<div align="center">
    <img width="442" height="211" alt="Snipaste_2026-06-15_19-06-03" src="https://github.com/user-attachments/assets/f5579874-62b9-4a27-86af-2927a7ed8cc6" />
</div>

- 小窗显示 Small window display
<div align="center">
   <img width="430" height="60" alt="image" src="https://github.com/user-attachments/assets/680d71e2-7ec5-4692-8730-d64476a67c0f" />
</div>

- 表盘显示 Watch face display
<div align="center">
<img width="339" height="309" alt="image" src="https://github.com/user-attachments/assets/40f2fb7e-da6f-4bc3-b814-0b7213565be0" />
</div>

- 可配置自动启动（linux、windows支持） Configurable auto-start (supported on Linux and Windows)

<div align="center">
   <img width="474" height="179" alt="image" src="https://github.com/user-attachments/assets/b4b4cd74-5688-4351-80eb-c41dabcfd946" />

</div>


# 注意  Note：
🛡️ macOS 版程序没有数字签名，如果你从[github release](https://github.com/359193585/DualClock/releases/tag/v2.1.0) 下载了该 tar.gz 包，解压后，macOS会系统会自动给该文件添加一个名为 com.apple.quarantine 的扩展属性，即“来源不明”的标签。The macOS version of the program is not digitally signed. If you downloaded the tar.gz package from the GitHub Release, after extraction, macOS will automatically add an extended attribute named com.apple.quarantine to the file, which is the "unknown origin" tag.
- 当你尝试运行带有此标签的应用时，Gatekeeper 会进行更严格的安全检查。When you try to run an application with this tag, Gatekeeper will perform stricter security checks.
- 对于未通过 Apple 公证（Notarization）的应用，系统就会弹出“已损坏，无法打开”的提示。For applications that have not passed Apple Notarization, the system will pop up a prompt saying "is damaged and can't be opened".
- 这个问题本质上是安全机制导致的“假损坏”，而非文件真的坏了。This issue is essentially "false damage" caused by the security mechanism, not that the file is actually broken.
- 你可以移除隔离属性，在终端中，针对单个 .app 文件，使用 xattr 命令移除该属性。You can remove the quarantine attribute by using the xattr command in the terminal for a single .app file to remove this attribute.
 

```
xattr -d com.apple.quarantine /path/to/DualClock.app
```
