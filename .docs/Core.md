<div align=center>

# System Core
### *The System core  is placed in the [Analyzer.cs](../Src/Windows%20Disk%20Analyzer/Analyzer.cs) file*

</div>

## `Struct Files_presentor`

> This struct purpose is to hold all the need information about any file so that we could more simply represent it at the UI
>> ```c#
>>    public string Name; // The Name of the Directory/File
>>    public string Attributes; // System attributes of the file
>>    public long size; // The Total Size of the file/Directory
>>    public DirectoryInfo dir_info; // [If not dir = null] will contain the object of the dir from System.IO;
>> ```

<br/>

# Analyzer
> The Analyzer is the main class at the core and should take care of the drive mapping include showing the user the giving the window everything he needs 
> ### **How it works**
> the analyzer doing a recursive mapping to every folder in the system and summing up the sizes of everything and also saving every big file in dictionary so it wouldn't need to remap it again 
>>*By saving only the big folders that already been mapped i am able to optimize the speed of the system and also not using too much memory like saving all the folders at the drive*

# How To Use:
> * First you need to create new object of the class [Analyzer](#analyzer) and to specify the path of the directory you want to map<br/>
`` By doing that The object will start mapping the whole path that has been specify ``
> * After the object has finished the mapping you will be able to get the whole directories in the directory and their sizes by calling the function [Get_elements_in_dir()](#get_elements_in_dir)

<br/>
<br/>

# Methods
## Get_elements_in_dir()
> TODO

## get_Dir_Size()
> TODO

## get_Formated_size()
> TODO

## GetParent()
> TODO

## GetCurrentPath()
> TODO

## BytesToString(long byteCount)
> TODO

## Get_elements_in_dir()
> TODO

## \<private\> DeepSizeScan(DirectoryInfo dir_info)