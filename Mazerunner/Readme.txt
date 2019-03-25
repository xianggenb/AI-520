This is a C# Mazerunner program with bitmap UI, built with Visual stuido 2017

The path finding source code is in Form1.cs and different algorithms & Utility functions are seperated with
regions 

Priority_Queue for C# Tuple(pair in C++) is self-implemented, source code is in PriorityQueue.cs


There are two different ways of running this program, 

First download the zip file and run Mazerunner.sln with proper path

Second, run Mazerunner\Mazerunner\bin\Debug\Mazerunner.exe directly

The GUI is user-friendly and there is no need to talk about how to run the algorithms
Just simple select/key in  and press button to run

If there exist a path to the end, all the corresponding info will be posted at the bottom
Expanded node will be marked as orange and the path will be marked as green
If not , there will be dead end alert;

Extra features:
1. After generate a new maze, you can right click on one of the node square on the bitmap -> choose open it
   or close it.(a contentMenuStrip will pop up)

2. You can save current maze by click the save button and upload it again.
   Be careful not to save the maze after path finding, since all the expanded node will be marked as close then

3. each new generated/uploaded maze can only be used once. the path finding algorithm will be automatically disabled after 
   search. 


