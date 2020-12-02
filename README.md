# A simple mining and crafting game

Contains a small world being generated, allows for building blocks and mining.
The world generation itself is done using compute shaders, while the rest of the
sandbox is running inside the main Unity thread.

The game world state modifications are saved on Windows to a file
in ```%APPDATA%\LocalLow\tombuben\MineGrip```,
feel free to delete the folder or the files to reset the world state.

Controls:

 * WSAD + Mouse to move
 * Space to jump
 * Left click hold to break
 * Right click to build
 * Scroll wheel to select currently building block

# Potential optimalisation

There are several potential fixes/optimalisation to the current implementation

 * The textures are currently packed too tightly, causing visible lines between
    some block types when viewed from certain distances
 * Although the world is generated inside a compute shader, the mesh itself is
    generated on the main CPU thread, with some inefficient allocations.
    Parts of the mesh generation can be offloaded on a compute shader, or the
    entire thing can be done in a separate thread.
 * The chunk meshes generated on the CPU are watertight, which means they contain
    walls on the outer edge of the chunk. Those tris won't be viible for the most
    part, so neighbor-aware mesh generation would simplify those meshes a lot.
 * The save file format is relatively inefficient, since I'm just dumping the voxel
    array without any compression. Even basic compression such as RLE would make
    the save file size much much smaller.
