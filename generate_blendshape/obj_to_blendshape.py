import sys
import numpy as np
import glob

"""
USAGE: python obj_to_blendshape.py {folder with expression obj} {original obj file} {blendshapes file}
EXAMPLE: python3 obj_to_blendshape.py expressions/ NeutralHead.obj blendshapes.txt
"""

obj_folder = sys.argv[1] # folder containing the expression obj files
base_file = sys.argv[2] # the base expression
blendshapes_file = sys.argv[3]

baseshape = []
with open(base_file) as baseshape_file:
    for line in baseshape_file:
        line = line.replace('\n', '').split(' ')
        if line[0] != 'v': continue
        baseshape.append(float(line[1]))
        baseshape.append(float(line[2]))
        baseshape.append(float(line[3]))
baseshape = np.array(baseshape)

new_shapes = []
for filepath in glob.iglob(obj_folder + '/*.obj'):
    filename = filepath.split('/')[-1].replace('.obj', '')
    newshape = []
    with open(filepath) as newshape_file:
        for line in newshape_file:
            line = line.replace('\n', '').split(' ')
            if line[0] != 'v': continue
            newshape.append(float(line[1]))
            newshape.append(float(line[2]))
            newshape.append(float(line[3]))
    new_shapes.append((filename, np.array(newshape)))

with open(blendshapes_file, 'w') as file:
    for name, newshape in new_shapes:
        file.write(name + ' ' + ' '.join([str(round(x, 4)) for x in newshape - baseshape]) + '\n')
        file.write(name + '_negative ' + ' '.join([str(round(x, 4)) for x in (newshape - baseshape)*-1]) + '\n')
