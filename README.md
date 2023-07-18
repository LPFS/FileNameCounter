# Information about this assignment

This task has not been executed to find a swift implementation, but rather the goal has been to 
demonstrate a wide variety of techniques, which the author hopes will be useful.

## Assumptions
### Problem definition
It is assumed that matches can be interleaved, e.g.
searching for string "aba" in "ababa" should generate two matches. 
### Arguments
1. File name argument is not obliged to have an extension.
1. Exactly one argument is to be tolerated. 
1. A file name consisting of just an extension is an illegal argument.
1. Argument is given as an absolute path.
### Encoding
It is assumed that files are encoded in UTF-8, or have the BOM correctly set.
### Test data
It is assumed that the test data will contain extremely long lines.
## Analysis of original code
1. Basic error handling is missing for the arguments.
1. Error handling is missing for file access.
1. The file name is assumed to have a dot.
1. The file names first dot is used for separating the main part, should be the last.
1. If the file holds really long lines, or for that matter, only one, reading the whole line causes excessive memory usage.
1. Only one hit per line is counted.
1. Using an int as counter opens the possibility of an overflow for very special conditions, e.g. a file named a.txt consisting of only a:s (encoded using one byte under utf-8) of size 3 gb. This is a bit unlikely, but still, using type long simply makes it impossible.
## Some notes on searching
The made assumptions on matching and test data gave some interesting consequences. First of all we
must read a limited number of chars at a time and secondly we have to find a way to find interleaved matches.
Two implementations are included, one consuming a character at a time (from a buffer) and keeping a state
machine of the possible stages of matching. This turned out to be rather simple to implement, but was really
slow on sparse data. The better choice was to use the new Span type and built in search. Regex was also 
tried but it does not find interleaved matches (at least not in the .NET implementation).

# Instructions
The implementation is in .NET 7 and uses some new features in C# 12 including spans and 
generic maths. The program is ran with one argument, an absolute file path.
## List of used top level nugets

Project 'FileNameCounter' has the following package references
   [net7.0]:
   Top-level Package                               Requested   Resolved  
   > Microsoft.Extensions.DependencyInjection      7.0.0       7.0.0  
   > Microsoft.Extensions.Hosting                  7.0.1       7.0.1  
   > System.IO.Abstractions                        19.2.29     19.2.29  

Project 'Tests' has the following package references
   [net7.0]:
   Top-level Package                               Requested   Resolved  
   > coverlet.collector                            3.2.0       3.2.0  
   > Microsoft.Extensions.DependencyInjection      7.0.0       7.0.0  
   > Microsoft.Extensions.Hosting                  7.0.1       7.0.1  
   > Microsoft.NET.Test.Sdk                        17.5.0      17.5.0  
   > Moq                                           4.18.4      4.18.4  
   > Moq.AutoMock                                  3.5.0       3.5.0  
   > NUnit                                         3.13.3      3.13.3  
   > NUnit.Analyzers                               3.6.1       3.6.1  
   > NUnit3TestAdapter                             4.4.2       4.4.2  
   > System.IO.Abstractions                        19.2.29     19.2.29  
   > System.IO.Abstractions.TestingHelpers         19.2.29     19.2.29  

 

