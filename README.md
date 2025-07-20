ClipboardSensor is an application that beeps at you when copying to clipboard succeeds or fails.

Specifically it plays one of the following sound effects:

* switch.wav if it succeeded and has non-empty text,

* switch2.wav if it succeeded and has binary data,

* bump.wav if it succeeded but the clipboard is now empty (or only contains "Chromium internal source RFH token, Chromium internal source URL")

If no sound effect plays, no clipboard update happened.

I wrote this application because Discord has a race condition where it fails to copy to clipboard sometimes. (For example, if you edit a message and quickly hit Ctrl+A Ctrl+C your clipboard will be emptied, but if you pause slightly first it works.) Now I always know for sure if a clipboard copy succeeded or failed!

I wrote this for personal use, but you can use it if you want (see Releases).