# A simple mining and crafting game

![](Screen.png?raw=true)


Contains a small world being generated, allows for building blocks and mining.
The world generation itself is done using compute shaders, while the rest of the
sandbox is running inside the main Unity thread. You can download the latest build in
[the releases tab](https://github.com/tombuben/MineGrip/releases).

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
    some block types when viewed from certain distances. A differently packed
    atlas with a few px between each block texture would fix this.
 * Although the world is generated inside a compute shader, the mesh itself is
    generated on the main CPU thread, with some inefficient reallocations the GC
    has to clean(since we don't know ahead of time how many vertices a chunk will have).
    It isn't really that noticable, but it takes ~10ms per chunk, so making it faster
    could still be useful. Parts of the mesh generation can be offloaded to a
    compute shader, or the entire thing can be done in a separate thread
    (both can't be done at once afaik, since only the main thread can work with
    the gpu).
 * The chunk meshes generated on the CPU are watertight, which means they contain
    walls on the outer edge of the chunk. Those tris won't be visible for the most
    part, so neighbor-aware chunk mesh generation would simplify those meshes a lot.
 * The save file format is relatively inefficient, since I'm just dumping the voxel
    array without any compression. Even basic custom compression such as RLE would make
    the save file size much much smaller.

# Textures

The textures are from Kenney (kenney.nl), who provides loads of free assets released
under the CC0 public domain license. He's a cool guy, check him out.
