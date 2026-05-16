# README #

SQL Hunting Dog — fast database object search panel for SSMS.

Compatible with:
* **SSMS 18** (2019)
* **SSMS 22** (2022) ✓

## Install

[Download the latest release.](https://github.com/pstraszak/sql-hunting-dog/releases)

**Step 1 — Unblock the zip**

Right-click the downloaded zip in Windows Explorer → Properties. If an `Unblock` button or checkbox is visible, click it before extracting.

**Step 2 — Copy the extension folder**

Extract the zip. Inside you will find a `HuntingDog` folder containing:

```
HuntingDog.dll
HuntingDog.dll.config
HuntingDog.pkgdef
NLog.dll
Xceed.Wpf.Toolkit.dll
```

Copy the entire `HuntingDog` folder (not just its contents) into the SSMS Extensions directory for your version:

| SSMS version | Extensions path |
|---|---|
| SSMS 18 (2019) | `C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\HuntingDog` |
| SSMS 22 (2022) | `C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions\HuntingDog` |

If a previous version is already installed, replace the folder entirely.

**Step 3 — Restart SSMS**

SQL Hunting Dog will appear under the **Tools** menu after restart.

## Contribution guidelines

* Create a separate branch and then generate a merge request
