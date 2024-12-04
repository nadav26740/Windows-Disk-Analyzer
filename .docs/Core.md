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
>> *By saving only the big folders that already been mapped i am able to optimize the speed of the system and also not using too much memory like saving all the folders at the drive*

# How To Use:
> * First you need to create new object of the class [Analyzer](#analyzer) and to specify the path of the directory you want to map<br/>
`` By doing that The object will start mapping the whole path that has been specify ``
> * After the object has finished the mapping you will be able to get the whole directories in the directory and their sizes by calling the function [Get_elements_in_dir()](#get_elements_in_dir)

<br/>
<br/>

# Methods
## Get_elements_in_dir()
> `` public List<Files_presentor> Get_elements_in_dir() `` <br/>
> Returns a list of [File_Presentor](#struct-files_presentor) that represent all the elements in the dir

## get_Dir_Size()
> `` public long get_Dir_Size() `` <br/>
> Returns the size of the current directory in bytes

## get_Formated_size()
> `` public string get_Formated_size() `` <br/>
> Returning the current directory size in Formatted scale string (B, KB, MB ...)
> By using the function [BytesToString](#static-bytestostringlong-bytecount) 

## GetParent()
> `` public string GetParent() `` <br/>
> Returning a string contains the path to parent directory of the current directory  

## GetCurrentPath()
> ``public string GetCurrentPath()`` <br/>
> Returning a string that contains the path to the current directory 

## \<Static\> BytesToString(long byteCount)
> `` public static string BytesToString(long byteCount) `` <br/>
> Getting a long int that should represent bytes and returning a string presenting the size in scale format (KB, MB, GB, TB) 

## \<private\> DeepSizeScan(DirectoryInfo dir_info)
> `` static long DeepSizeScan(DirectoryInfo dir_info) `` <br/>
> ***The Deep scan is the core function of the Mapping system***<br/>
> the function purpose is to run size mapping on every directory and file in the tree recursively by recall the function on each directory at the File tree and return the current root size <br/><br/>
> The function also keep in dictionary all the directories that has been scanned and their size is over [BIG_DIRECTORY_SIZE_GB](#static-field-float-big_directory_size_gb) so it won't remap big directories and by that save optimize the runtime of navigating between different directories without extreme memory usage. <br/>

## \<Static Field\> float BIG_DIRECTORY_SIZE_GB 
> Determinate the size of what to consider as big directory in GB
>> *used in [DeepSizeScan](#private-deepsizescandirectoryinfo-dir_info) for optimization*