<img width="447" height="160" alt="image" src="https://github.com/user-attachments/assets/442287b0-59f3-423c-8f05-81457c80aa42" />

ClipboardSensor is a Windows application that beeps at you when copying to clipboard succeeds or fails.

Specifically it plays one of the following sound effects:

* switch.wav if it succeeded and has non-empty text,

* switch2.wav if it succeeded and has binary data,

* bump.wav if it succeeded but the clipboard is now empty (or only contains "Chromium internal source RFH token, Chromium internal source URL")

If no sound effect plays, no clipboard update happened.

ClipboardSensor v1.1 additionally adds a clipboard history with the following functions:

* Alt+Z (Undo) / Alt+X (Redo) updates the clipboard. Works even if ClipboardSensor is not focused. You can also use the left numeric edit to go to a specific entry.

* The right numeric edit shows how many entries are currently saved; and if edited, you can edit the maximum history size (defaults to 32).

*  If a clipboard change is sensed, the 'redo buffer' is not deleted. (Since it's not strictly a measurement of events happening in order and it's just a place where old values are stored, I realized I could write it this way, so entries on the 'redo buffer' aren't deleted if a new copy happens.) When the history is exceeded, the furthest away entry on the longest 'tail' is deleted (if the 'undo buffer' is longer entry 0 is deleted; if the 'redo buffer' is longer the last entry is deleted).

*  Debounce (in milliseconds) is how long two consecutive clipboard changes have to be apart before they constitute two different history entries. Set to -1 to have it always succeed. (Some copies (like the URL bar in Chrome) make one or more copies in quick succession.)

ClipboardSensor v1.2 adds the following functions:

* Alt+C (Trim Text) takes the current clipboard history entry, removes all data formats except UnicodeText (or if that's absent, Text), trims the resulting string and updates the clipboard history and Windows clipboard with the new value. It trims non-text AND trims text! A double trim!

* 'Hook Hotkeys' lets you disable the hotkeys if you need Alt+foo to work properly in another program.

I wrote this application because Discord has a race condition where it fails to copy to clipboard sometimes. (For example, if you edit a message and quickly hit Ctrl+A Ctrl+C your clipboard will be emptied, but if you pause slightly first it works.) (I suspect this is a Chromium bug and not a Discord bug.) Now I always know for sure if a clipboard copy succeeded or failed!

I wrote this for personal use, but you can use it if you want (see Releases).
