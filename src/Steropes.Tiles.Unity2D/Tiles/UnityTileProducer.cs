using Steropes.Tiles.DataStructures;
using Steropes.Tiles.TexturePack.Grids;
using Steropes.Tiles.TexturePack.Operations;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityTileProducer : TileProducerBase<UnityTile, UnityTexture, UnityRawTexture>

    {
        public UnityTileProducer(ITextureOperations<UnityTexture> textureOperations, 
                                 ITextureAtlasBuilder<UnityTexture> atlasBuilder): base(textureOperations, atlasBuilder)
        {
        }

        protected override UnityTile CreateTile(string tag, UnityTexture texture, IntDimension tileSize, IntPoint anchor)
        {
            return new UnityTile(tag, texture, tileSize, anchor);
        }
    }
}