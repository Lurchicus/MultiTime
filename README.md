## MultiTime

MultiTime is a C#.Net WinForms program that will show the current time across multiple time zones. The
time zones displayed are defined in a .json file that allow you to customize the times listed in the UI.

The main reason I wrote this was to teach myself several concepts.

- Increase my skill working in C#
- Learn how to read program data from a .json file
- Learn how to dynamicly generate UI elements using C#
- Learn how to create an MSIX installer for installing a WinForms application

The install package I created can be installed but it would require installing my public self signed certificate
to the system you run the program on (along with the associated risks). Better to get the program source
and make your own install package.

## Notes

I have plans to add a form that will allow the user to read, edit and save the .json files used to configure
the UI.

I will also be adding a viewer for the GNU 3 license at a future date.

This project is licensed under the GNU 3 license. I'll formalize that in this repository later.
