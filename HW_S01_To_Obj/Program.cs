using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HW_S01_To_Obj
{

    class Program
    {
        readonly static float modelScaleDown = 0.0225f;

        class Vector3
        {
            public float x;
            public float y;
            public float z;

            public Vector3()
            {
                x = 0;
                y = 0;
                z = 0;
            }

            public Vector3(float X, float Y, float Z)
            {
                x = X;
                y = Y;
                z = Z;
            }

            float sin(float angle)
            {
                return MathF.Sin(angle);
            }

            float cos(float angle)
            {
                return MathF.Cos(angle);
            }

            public void RotateVectorByMatrix(RotationMatrix matrix)
            {
                float angleA = matrix.z.y - matrix.y.z;
                float angleB = matrix.x.z - matrix.z.x;
                float angleC = matrix.y.x - matrix.x.y;

                RotateByX(angleA);
                RotateByY(angleB);
                RotateByZ(angleC);

                /*float X = x;
                float Y = y;
                float Z = z;

                x = 
                    X * cos(matrix.x.x) - sin(matrix.x.y)
                    +
                    X * cos(matrix.x.x) + Z * sin(matrix.x.z);

                y = 
                    X * sin(matrix.y.x) + Y * cos(matrix.y.y)
                    +
                    Y * cos(matrix.y.y) - Z - sin(matrix.y.z);

                z = 
                    -X * sin(matrix.z.x) + Z * cos(matrix.z.z)
                    +
                    Y * sin(matrix.z.y) + Z * cos(matrix.z.z);*/
            }

            public void RotateByX(float alpha)
            {
                float Y = y;
                float Z = z;

                y = Y * cos(alpha) - Z * sin(alpha);
                z = Y * sin(alpha) + Z * cos(alpha);
            }

            public void RotateByY(float alpha)
            {
                float X = x;
                float Y = y;
                float Z = z;

                x = X * cos(alpha) - Y * sin(alpha);
                z = -X * sin(alpha) + Z * cos(alpha);
            }

            public void RotateByZ(float alpha)
            {
                float X = x;
                float Y = y;

                x = X * cos(alpha) - Y * sin(alpha);
                y = X * sin(alpha) + Y * cos(alpha);
            }

            public static Vector3 operator +(Vector3 a, Vector3 b)
            {
                Vector3 vec = a;

                vec.x += b.x;
                vec.y += b.y;
                vec.z += b.z;

                return vec;
            }

            public override string ToString()
            {
                return x + " " + y + " " + z;  
            }
        }

        class RotationMatrix
        {
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;

            public RotationMatrix()
            {
                x = new Vector3();
                y = new Vector3();
                z = new Vector3();
            }

            public RotationMatrix(float[] matrix)
            {
                x = new Vector3(matrix[0], matrix[1], matrix[2]);
                y = new Vector3(matrix[3], matrix[4], matrix[5]);
                z = new Vector3(matrix[6], matrix[7], matrix[8]);
            }
        }

        struct Quad
        {
            public int[] vertIds;//4
            public int[] texVertIds; //4
            public float[] normals; //12

            public string textureName;
            public float glow;
            public Vector3 flat;

            public string opacityMode;
            public float opacityStrength;

            public string enviroMapTex;
            public float enviroMapStrength;
        }

        struct Tris
        {
            public int[] vertIds; //3
            public int[] texVertIds; //3
            public float[] normals; //9

            public string textureName;
            public float glow;
            public Vector3 flat;

            public string opacityMode;
            public float opacityStrength;

            public string enviroMapTex;
            public float enviroMapStrength;
        }

        class Object
        {
            public string name = "";

            public RotationMatrix rotMatrix;

            public int frameAnimStart = 0;

            public Vector3 position;

            public List<Vector3> verts; // 3
            public List<float[]> texVerts; // 4

            public List<Quad> quads = new List<Quad>();
            public List<Tris> tris = new List<Tris>();

            public List<string> textureFilenames = new List<string>();
        }

        static int GetIntFromBracketLine(string line, int ignoreLetters)
        {
            line = line.Substring(ignoreLetters);
            line = line.Substring(0,line.Length-1);
            return int.Parse(line);
        }

        static void ReadObject(string[] objectData, out Object newObj)
        {
            newObj = null;

            try
            {
                newObj = new Object();

                int RL = 0;

                // 0 Name

                newObj.name = objectData[RL].Substring(9);
                RL++;

                // 0.5 Frame start

                if (objectData[RL].StartsWith("Frame : "))
                {
                    objectData[RL] = objectData[RL].Substring(8);
                    newObj.frameAnimStart = int.Parse(objectData[RL]);
                    RL++;
                }

                // 1 Matrix vars

                objectData[RL] = objectData[RL].Substring(9);
                string[] rawMatrixVars = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);

                float[] matrixVars = new float[12];
                for (int i = 0; i < matrixVars.Length; i++)
                    matrixVars[i] = float.Parse(rawMatrixVars[i]);

                newObj.rotMatrix = new RotationMatrix(matrixVars);
                newObj.position = new Vector3(matrixVars[9] * modelScaleDown, matrixVars[10] * modelScaleDown, matrixVars[11] * modelScaleDown);

                newObj.position.RotateByX(MathF.PI/2);
                newObj.position.RotateByZ(MathF.PI);

                //newObj.position.RotateVectorByMatrix(newObj.rotMatrix);

                // 2 Verts

                RL++;

                int amountOfVerts = GetIntFromBracketLine(objectData[RL], 7);
                List<Vector3> verts = new List<Vector3>();

                RL+=2;

                for (int i = 0; i < amountOfVerts; i++)
                {
                    string[] rawVert = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    float[] vert = new float[3];

                    for (int j = 0; j < vert.Length; j++)
                        vert[j] = float.Parse(rawVert[j]);

                    Vector3 vec = new Vector3(vert[0] * modelScaleDown, vert[1] * modelScaleDown, vert[2] * modelScaleDown);

                    vec.RotateByX(MathF.PI/2);
                    vec.RotateByZ(MathF.PI);

                    //vec.RotateVectorByMatrix(newObj.rotMatrix);

                    verts.Add(vec);

                    RL++;
                }

                newObj.verts = verts;

                // 3 Texture verts

                RL++;

                int amountOfTexVerts = GetIntFromBracketLine(objectData[RL], 15);
                List<float[]> texVerts = new List<float[]>();

                RL += 2;

                for (int i = 0; i < amountOfTexVerts; i++)
                {
                    string[] rawVert = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    float[] vert = new float[2];

                    for (int j = 0; j < vert.Length; j++)
                        vert[j] = float.Parse(rawVert[j]);

                    texVerts.Add(vert);

                    RL++;
                }

                newObj.texVerts = texVerts;

                // texture filenames

                List<string> textureFilenames = new List<string>();

                // 4 Quads

                RL++;

                int amountOfQuads = GetIntFromBracketLine(objectData[RL], 7);
                List<Quad> quads = new List<Quad>();

                RL += 2;

                for (int i = 0; i < amountOfQuads; i++)
                {
                    Quad newQuad = new Quad();

                    string[] raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    int[] vertIds = new int[4];

                    for (int j = 0; j < vertIds.Length; j++)
                        vertIds[j] = int.Parse(raw[j]) + 1;

                    newQuad.vertIds = vertIds;


                    RL++;

                    if (objectData[RL].StartsWith(" Flat : "))
                    {
                        objectData[RL] = objectData[RL].Substring(8);

                        raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 flat = new Vector3(float.Parse(raw[0]), float.Parse(raw[1]), float.Parse(raw[2]));

                        newQuad.flat = flat;
                        RL++;
                    }

                    if (objectData[RL].StartsWith(" Texture :"))
                    {
                        objectData[RL] = objectData[RL].Substring(12);
                        raw = objectData[RL].Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

                        newQuad.textureName = raw[0].Replace(' ','_');
                        textureFilenames.Add(newQuad.textureName);

                        raw = raw[1].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                        vertIds = new int[4];

                        for (int j = 0; j < vertIds.Length; j++)
                            vertIds[j] = int.Parse(raw[j]) + 1;

                        newQuad.texVertIds = vertIds;

                        RL++;
                    }

                    if(objectData[RL].StartsWith(" Enviromap : "))
                    {
                        objectData[RL] = objectData[RL].Substring(14);

                        raw = objectData[RL].Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

                        newQuad.enviroMapTex = raw[0].Replace(' ', '_');

                        raw[1] = raw[1].Trim();

                        newQuad.enviroMapStrength = float.Parse(raw[1]);

                        RL++;
                    }

                    if(objectData[RL].StartsWith("  Opacity : "))
                    {
                        objectData[RL] = objectData[RL].Substring(12);

                        raw = objectData[RL].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        newQuad.opacityMode = raw[0];
                        newQuad.opacityStrength = float.Parse(raw[1]);
                        RL++;
                    }

                    if (objectData[RL].StartsWith("  Glow : "))
                    {
                        objectData[RL] = objectData[RL].Substring(9);
                        newQuad.glow = float.Parse(objectData[RL]);
                        RL++;
                    }

                    if (!objectData[RL].StartsWith(" Normals :"))
                    {
                        quads.Add(newQuad);
                        continue;
                    }

                    objectData[RL] = objectData[RL].Substring(11);
                    raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    float[] normals = new float[12];

                    for (int j = 0; j < normals.Length; j++)
                        normals[j] = float.Parse(raw[j]);

                    newQuad.normals = normals;

                    quads.Add(newQuad);

                    RL++;
                }

                newObj.quads = quads;

                // 5 Tris

                RL++;

                int amoundOfTris = GetIntFromBracketLine(objectData[RL], 6);
                List<Tris> tris = new List<Tris>();

                RL += 2;

                for (int i = 0; i < amoundOfTris; i++)
                {
                    Tris newTris = new Tris();

                    string[] raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    int[] vertIds = new int[3];

                    for (int j = 0; j < vertIds.Length; j++)
                        vertIds[j] = int.Parse(raw[j]) + 1;

                    newTris.vertIds = vertIds;


                    RL++;

                    if (objectData[RL].StartsWith(" Flat : "))
                    {
                        objectData[RL] = objectData[RL].Substring(8);

                        raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                        Vector3 flat = new Vector3(float.Parse(raw[0]), float.Parse(raw[1]), float.Parse(raw[2]));

                        newTris.flat = flat;
                        RL++;
                    }

                    if (objectData[RL].StartsWith(" Texture :"))
                    {
                        objectData[RL] = objectData[RL].Substring(12);
                        raw = objectData[RL].Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

                        newTris.textureName = raw[0].Replace(' ', '_');
                        textureFilenames.Add(newTris.textureName);


                        raw = raw[1].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                        vertIds = new int[3];

                        for (int j = 0; j < vertIds.Length; j++)
                            vertIds[j] = int.Parse(raw[j]) + 1;

                        newTris.texVertIds = vertIds;

                        RL++;
                    }

                    if (objectData[RL].StartsWith(" Enviromap : "))
                    {
                        objectData[RL] = objectData[RL].Substring(14);

                        raw = objectData[RL].Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);

                        newTris.enviroMapTex = raw[0].Replace(' ', '_');

                        raw[1] = raw[1].Trim();

                        newTris.enviroMapStrength = float.Parse(raw[1]);

                        RL++;
                    }

                    if (objectData[RL].StartsWith("  Opacity : "))
                    {
                        objectData[RL] = objectData[RL].Substring(12);

                        raw = objectData[RL].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        newTris.opacityMode = raw[0];
                        newTris.opacityStrength = float.Parse(raw[1]);
                        RL++;
                    }

                    if (objectData[RL].StartsWith("  Glow : "))
                    {
                        objectData[RL] = objectData[RL].Substring(9);
                        newTris.glow = float.Parse(objectData[RL]);
                        RL++;
                    }

                    if (!objectData[RL].StartsWith(" Normals :"))
                    {
                        tris.Add(newTris);
                        continue;
                    }

                    objectData[RL] = objectData[RL].Substring(11);
                    raw = objectData[RL].Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
                    float[] normals = new float[9];

                    for (int j = 0; j < normals.Length; j++)
                        normals[j] = float.Parse(raw[j]);

                    newTris.normals = normals;

                    tris.Add(newTris);

                    RL++;
                }

                newObj.tris = tris;

                // Removes duplicates
                newObj.textureFilenames = textureFilenames.Distinct().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse an object \n "+ex);
            }
        }

        static List<Object> FindObjects(string fileData)
        {
            List<Object> objects = new List<Object>();

            string[] lines = fileData.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("Object"))
                    continue;

                for (int j = i + 1; j < lines.Length; j++)
                {
                    if(lines[j].StartsWith("Object") || lines[j].StartsWith("end"))
                    {
                        string[] parseObjectData = new string[j - i];

                        Array.Copy(lines, i, parseObjectData, 0, j - i);

                        try
                        {
                            ReadObject(parseObjectData, out Object newObj);
                            objects.Add(newObj);
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }

                        i = j;
                    }
                }
            }

            return objects;
        }

        static bool HasAnyTextures(List<Object> objects)
        {
            for (int i = 0; i < objects.Count; i++)
                if (objects[i].textureFilenames.Count > 0)
                    return true;

            return false;
        }

        static void WriteObjFile(List<Object> objects, string objFilePath, string texturesFilepath)
        {
            try
            {
                string output = "";

                int vertCounter = 0;
                int texVertCounter = 0;

                bool hasAnyTextures = HasAnyTextures(objects);

                if(hasAnyTextures)
                {
                    List<string> textures = new List<string>();

                    for (int i = 0; i < objects.Count; i++)
                        textures.AddRange(objects[i].textureFilenames);

                    textures = textures.Distinct().ToList();

                    for (int i = 0; i < textures.Count; i++)
                    {
                        output += "newmtl material_" + textures[i].Substring(0, textures[i].Length - 4) + '\n';
                        output += "Ns 0.000000\nKa 1.000000 1.000000 1.000000\nKd 1.000000 1.000000 1.000000\nKs 0.000000 0.000000 0.000000\nKe 0.000000 0.000000 0.000000\nNi 1.450000\nd 1.000000\nillum 1\n";
                        output += "map_Kd " + texturesFilepath + textures[i].Substring(0, textures[i].Length-4) + ".png\n\n";
                    }

                    string mtlPath = objFilePath.Substring(0, objFilePath.Length - 3) + "mtl";
                    StreamWriter writeMtl = new StreamWriter(mtlPath);
                    writeMtl.Write(output);
                    writeMtl.Close();

                    output = "mtllib " + Path.GetFileName(mtlPath) + "\n";
                }

                for (int i = 0; i < objects.Count; i++)
                {
                    output += "# objectName: " + objects[i].name + "\n";
                    output += "o " + objects[i].name + "\n";

                    if(hasAnyTextures && objects[i].textureFilenames.Count > 0)
                    {
                        string name = objects[i].textureFilenames[0];
                        output += "usemtl material_" + name.Substring(0, name.Length-4) + "\n";
                    }

                    // verts info
                    for (int j = 0; j < objects[i].verts.Count; j++)
                    {
                        //objects[i].verts[j].RotateVector(objects[i].rotMatrix);
                        objects[i].verts[j] += objects[i].position;

                        output += "v " + objects[i].verts[j].ToString() + "\n";
                    }

                    // texture verts
                    for (int j = 0; j < objects[i].texVerts.Count; j++)
                        output += "vt " + objects[i].texVerts[j][0].ToString() + " " + objects[i].texVerts[j][1].ToString() + "\n";

                    // faces

                    // quads
                    for (int j = 0; j < objects[i].quads.Count; j++)
                    {
                        if (objects[i].quads[j].texVertIds == null) // No textures...
                        {
                            output += "f " + (objects[i].quads[j].vertIds[0] + vertCounter).ToString()  +
                            " " + (objects[i].quads[j].vertIds[1] + vertCounter).ToString() +
                            " " + (objects[i].quads[j].vertIds[3] + vertCounter).ToString() +
                            " " + (objects[i].quads[j].vertIds[2] + vertCounter).ToString() +
                            "\n";
                        }
                        else
                        {
                            output += "f " + (objects[i].quads[j].vertIds[0] + vertCounter).ToString() + "/" + (objects[i].quads[j].texVertIds[0] + texVertCounter).ToString() +
                            " " + (objects[i].quads[j].vertIds[1] + vertCounter).ToString() + "/" + (objects[i].quads[j].texVertIds[1] + texVertCounter).ToString() +
                            " " + (objects[i].quads[j].vertIds[3] + vertCounter).ToString() + "/" + (objects[i].quads[j].texVertIds[3] + texVertCounter).ToString() +
                            " " + (objects[i].quads[j].vertIds[2] + vertCounter).ToString() + "/" + (objects[i].quads[j].texVertIds[2] + texVertCounter).ToString() +
                            "\n";
                        }
                    }

                    // tris
                    for (int j = 0; j < objects[i].tris.Count; j++)
                    {
                        if(objects[i].tris[j].texVertIds == null) // No textures...
                        {
                            output += "f " + (objects[i].tris[j].vertIds[0] + vertCounter).ToString() +
                            " " + (objects[i].tris[j].vertIds[2] + vertCounter).ToString() +
                            " " + (objects[i].tris[j].vertIds[1] + vertCounter).ToString() +
                            "\n";
                        }
                        else
                        {
                            output += "f " + (objects[i].tris[j].vertIds[0] + vertCounter).ToString() + "/" + (objects[i].tris[j].texVertIds[0] + texVertCounter).ToString() +
                            " " + (objects[i].tris[j].vertIds[2] + vertCounter).ToString() + "/" + (objects[i].tris[j].texVertIds[2] + texVertCounter).ToString() +
                            " " + (objects[i].tris[j].vertIds[1] + vertCounter).ToString() + "/" + (objects[i].tris[j].texVertIds[1] + texVertCounter).ToString() +
                            "\n";
                        }


                    }

                    vertCounter += objects[i].verts.Count;
                    texVertCounter += objects[i].texVerts.Count;
                }

                StreamWriter write = new StreamWriter(objFilePath);
                write.Write(output);
                write.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write to file! \n " + ex);
            }
        }

        static void ConvertFolder(string folderPath)
        {
            string outputDirectory = AppContext.BaseDirectory + "Extraction";  

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);


            string texturesDir = outputDirectory + "\\Textures";

            if (!Directory.Exists(texturesDir))
                Directory.CreateDirectory(texturesDir);


            string[] s01Files = Directory.GetFiles(folderPath, "*.s01", SearchOption.AllDirectories);

            for (int i = 0; i < s01Files.Length; i++)
                ConvertFile(s01Files[i], outputDirectory, true);
        }

        static void ConvertFile(string filePath)
        {
            ConvertFile(filePath, "", false);
        }

        static void ConvertFile(string filePath, string outputFolder, bool useTextureFolder)
        {
            try
            {
                string extension = Path.GetExtension(filePath);

                if (extension == "")
                    filePath += ".S01";

                if (!File.Exists(filePath))
                    throw new Exception("File does not exist!");

                StreamReader read = new StreamReader(filePath);
                string fileData = read.ReadToEnd();
                read.Close();

                List<Object> objects = FindObjects(fileData);

                string outputFilepath;
                string filename = Path.GetFileNameWithoutExtension(filePath).Replace(' ', '_');

                if (outputFolder != "")
                    outputFilepath = outputFolder + "\\" + filename + ".obj";
                else
                    outputFilepath = filePath + "\\..\\" + filename + ".obj";

                if(useTextureFolder)
                    WriteObjFile(objects, outputFilepath, "Textures/");
                else
                    WriteObjFile(objects, outputFilepath, "");

                Console.WriteLine("File {"+ filename + "} parsed! Amount of objects converted: {" + objects.Count + "}");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to convert a file! \n" + filePath + "\n" + ex);
            }
        }

        static void ShowS01FilesInCurrentDir()
        {
            string[] files = Directory.GetFiles(AppContext.BaseDirectory, "*.s01");

            string output = "Found .S01 files in .exe dir:\n";
            for (int i = 0; i < files.Length; i++)
                output += Path.GetFileName(files[i]) + '\n';

            if (files.Length == 0)
                return;

            Console.WriteLine(output);
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string pathToConvert = args[0];

                if (Directory.Exists(pathToConvert))
                    ConvertFolder(pathToConvert);
                else if (File.Exists(pathToConvert))
                    ConvertFile(pathToConvert);

                return;
            }

            Console.Title = "S01_To_OBJ_Converter";

            do
            {
                Console.Clear();
                Console.WriteLine("Welcome to the .S01 to .obj converter! Made by GameDreamSt v1.0\n");

                ShowS01FilesInCurrentDir();

                Console.WriteLine("Enter either a single file or a project folder path to convert: ");

                string localPathToConvert = Console.ReadLine();

                string fullPath;

                if (localPathToConvert.Length > 1 && localPathToConvert[1] == ':') // Absolute path (because of the C:<---)
                    fullPath = localPathToConvert;
                else // otherwise a local path
                    fullPath = AppContext.BaseDirectory + localPathToConvert;

                if (Directory.Exists(fullPath))
                    ConvertFolder(fullPath);
                else
                {
                    string extension = Path.GetExtension(fullPath);

                    if (extension == "")
                        fullPath += ".S01";

                    if (File.Exists(fullPath))
                        ConvertFile(fullPath);
                }

                Console.WriteLine("Want to continue? [Y/N]: ");
                ConsoleKeyInfo answer = Console.ReadKey();

                if (answer.KeyChar == 'N' || answer.KeyChar == 'n') // Exit program
                    return;

            } while (true);
        }
    }
}