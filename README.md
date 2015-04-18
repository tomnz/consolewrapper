# ConsoleWrapper #

ConsoleWrapper is, in short, a 3D Windows command-line interface. At its most basic level, it is just a wrapper for the built-in Windows console (CMD) – hence the name. However, there is a custom shell built on top of it, and control is only relinquished to CMD for unrecognised commands.

In a nutshell, it works by redirecting output from any console application (usually cmd.exe) through ConsoleWrapper, slapping it onto 3D geometry, and animating that geometry. Currently, animation is basically smooth scrolling with a small amount of perspective, but potential is almost limitless.

Picture running multiple terminals at once, flipping through them like pages on a book, with full transparency, reflection, and physics effects. Lines drop into view, and discarded applications quietly fade away. These are just some of the possible concepts which can be achieved using a 3D environment such as ConsoleWrapper.

Note that ConsoleWrapper is very “work-in-progress”, and as such offers a pretty incomplete command line experience. No TAB completion is available yet, for example.