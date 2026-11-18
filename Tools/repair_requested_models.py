import bpy
import os
import sys

source = sys.argv[-2]
output = sys.argv[-1]

bpy.ops.wm.open_mainfile(filepath=source)
bpy.ops.object.select_all(action='DESELECT')

for obj in bpy.context.scene.objects:
    if obj.type != 'MESH':
        continue
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.0001)
    bpy.ops.mesh.delete(type='ONLY_FACE')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.quads_convert_to_tris(quad_method='BEAUTY', ngon_method='BEAUTY')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    obj.select_set(False)

os.makedirs(os.path.dirname(output), exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.export_scene.fbx(filepath=output, use_selection=True, object_types={'MESH'}, apply_unit_scale=True, bake_space_transform=False, add_leaf_bones=False)
