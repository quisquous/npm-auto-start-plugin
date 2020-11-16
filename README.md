# npm auto-start for OverlayPlugin

A plugin that automatically runs `npm start` on startup.
It uses the directory that the plugin is placed in
as the working directory to run this command in.

This is an OverlayPlugin plugin (rather than an ACT plugin)
in order to use its output log for errors.
