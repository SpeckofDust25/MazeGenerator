# MazeGenerator

## Summary
Maze Generator has many features, including 14 different algorithms, colors, cell size, wall size, number of horizontal cells (width), number of vertical cells (height), points (which determine start and end points), bias (vertical:horizontal), braiding (dead-end removal), paths (solution), image exporter, masking, and an intuitive user interface.

## Features

* 14 Different Maze Algorithms
  
  ![Alt Text](Gifs/Algorithms.gif)

* Masking
  
  ![Alt Text](Gifs/Masking.gif)

* A* Pathfinding and Path Generation
  
  ![Alt Text](Gifs/Pathfinding.gif)

* Modify Maze width and height
  
  ![Alt Text](Gifs/Width%20and%20Height.gif)

* Modify Cell and Wall size
  
  ![Alt Text](Gifs/Cell%20and%20Wall%20Size.gif)

* Modifications
  
  ![Alt Text](Gifs/Modifications.gif)

* Modify maze colors
  
  ![Alt Text](Gifs/Colors.gif)

* Intuitive Interface
  
  ![Alt Text](Gifs/Interface.gif)

* Export in .png format

## Other Details

Masking:

This feature allows you to draw on the maze, determining its shape. To draw on the maze, enable ‘Draw Mask’ and right-click/hold on the center cells to create a dead cell. To remove dead cells, just right-click/hold again. Note that when using the masking feature every cell needs to be reachable. If it is not then it will not generate a maze until that is fixed. 

Images:

Images are saved at the applications file location. If you want to create larger mazes, then set Cell Size and Wall Size to lower values like 1. 

WARNING: 

This application may crash if not used carefully. No limitations have been set on how large you can set the maze, other than the .png image itself. If the Width, Height, Cell Size, or Wall Size are too large, it will crash. Enabling points causes the Maze Generator to do a lot more work. Having a large number of cells will slow down the program. There is no multithreading, so if the mazes you create are large and use pathfinding, it will take a long time to process. This will freeze the main thread, meaning it will freeze the application until that process is finished and the image is generated. Multithreading may be added in the future to resolve this issue.

## Links

* [Maze Generator](https://cameronac.itch.io/maze-generator)
