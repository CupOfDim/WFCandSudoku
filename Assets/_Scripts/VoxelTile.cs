using System;
using UnityEngine;

public partial class VoxelTile : MonoBehaviour
{
    public float VoxelSize = 0.1f;
    public int TileSideVoxel = 8;

    [Range(1,100)]
    public int Weight = 50;

    public RotType Rotation;

    public enum RotType
    {
        OnlyRot,
        TwoRot,
        FourRot
    }

    [HideInInspector] public byte[] ColorsRight;
    [HideInInspector] public byte[] ColorsForward;
    [HideInInspector] public byte[] ColorsLeft;
    [HideInInspector] public byte[] ColorsBack;

    public void CalcSideColors()
    {
        ColorsRight = new byte[TileSideVoxel * TileSideVoxel];
        ColorsForward = new byte[TileSideVoxel * TileSideVoxel];
        ColorsLeft = new byte[TileSideVoxel * TileSideVoxel];
        ColorsBack = new byte[TileSideVoxel * TileSideVoxel];
        for(int y=0; y< TileSideVoxel; y ++)
        {
            for (int i = 0; i < TileSideVoxel; i++)
            {
                ColorsRight[y* TileSideVoxel + i] = GetVoxelColor(y, i, Direction.Right);
                ColorsForward[y * TileSideVoxel + i] = GetVoxelColor(y, i, Direction.Forward);
                ColorsLeft[y * TileSideVoxel + i] = GetVoxelColor(y, i, Direction.Left);
                ColorsBack[y * TileSideVoxel + i] = GetVoxelColor(y, i, Direction.Back);
            }
        };
    }

    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);

        byte[] colorsRightNew = new byte[TileSideVoxel * TileSideVoxel];
        byte[] colorsForwardNew = new byte[TileSideVoxel * TileSideVoxel];
        byte[] colorsLeftNew = new byte[TileSideVoxel * TileSideVoxel];
        byte[] colorsBackNew = new byte[TileSideVoxel * TileSideVoxel];

        for(int layer = 0; layer < TileSideVoxel; layer++)
        {
            for (int offset = 0; offset < TileSideVoxel; offset++)
            {
                colorsRightNew[layer * TileSideVoxel + offset] = ColorsForward[layer * TileSideVoxel + TileSideVoxel - offset - 1];
                colorsForwardNew[layer * TileSideVoxel + offset] = ColorsLeft[layer * TileSideVoxel + offset];
                colorsLeftNew[layer * TileSideVoxel + offset] = ColorsBack[layer * TileSideVoxel + TileSideVoxel - offset - 1];
                colorsBackNew[layer * TileSideVoxel + offset] = ColorsRight[layer * TileSideVoxel + offset];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsForward = colorsForwardNew;
        ColorsLeft = colorsLeftNew;
        ColorsBack = colorsBackNew;
    }

    private byte GetVoxelColor(int verticalLayer, int horizontalLayerOffset, Direction direction)
    {
        var meshCollider = GetComponentInChildren<MeshCollider>();

        float vox = VoxelSize;
        float half = VoxelSize / 2;

        Vector3 rayStart;
        Vector3 rayDir;

        if (direction == Direction.Forward)
        {
            rayStart = meshCollider.bounds.min +
                new Vector3(half + horizontalLayerOffset * vox, 0, -half);
            rayDir = Vector3.forward;
        }
        else if (direction == Direction.Right)
        {
            rayStart = meshCollider.bounds.min +
                new Vector3(-half, 0, half + horizontalLayerOffset * vox);
            rayDir = Vector3.right;
        }
        else if (direction == Direction.Left)
        {
            rayStart = meshCollider.bounds.max +
                new Vector3(half, 0, -half -(TileSideVoxel - horizontalLayerOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if(direction == Direction.Back)
        {
            rayStart = meshCollider.bounds.max +
                new Vector3(-half - (TileSideVoxel - horizontalLayerOffset - 1) * vox, 0, half);
            rayDir = Vector3.back;
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.forward/right/left/back", nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + half + verticalLayer * vox;

        //Debug.DrawRay(rayStart, direction*.1f, Color.red, 2);

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit hit, vox))
        {
            byte colorIndex = (byte)(hit.textureCoord.x * 256);
            return colorIndex;
        }

        return 0;
    }
}
