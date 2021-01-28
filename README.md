# HorizonCrypt
Animal Crossing: New Horizons Save Encryptor/Decryptor

## Building
you will need the latest .Net Framework (NOT Net standard 2.0) NHSE.Core.dll from [NHSE](https://github.com/kwsch/NHSE).

then u add a reference to that .dll in Visual Studio.

then u let it build.

the resulting .exe and .dll can be found in "./bin/Debug/" or "./bin/Release".

## Usage
HorizonCrypt \[-b\] \[-c|-d\] \<input\>

## Examples

#### Decrypt File
HorizonCrypt -d ./main.dat

#### Encrypt File
HorizonCrypt -c ./main_decrypted.dat

#### Decrypt Folder
HorizonCrypt -b -d ./

#### Encrypt Folder
HorizonCrypt -b -c ./
