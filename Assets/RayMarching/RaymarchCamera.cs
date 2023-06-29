    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RaymarchCamera : SceneViewFilter
{
    [SerializeField]
    private Shader _shader;

    private Material _raymarchMat;
    public Material _raymarchMaterial
    {
        get{
            if(!_raymarchMat && _shader)
            {
                _raymarchMat = new Material(_shader);
                _raymarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _raymarchMat;
        }
    }

    private Camera _cam;
    public Camera _camera{
        get{
            if(!_cam){
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }

    [Header("Setup")]
    public float _maxDistance;
    [Range(1, 300)] public int _maxIteration;
    [Range(0.1f, 0.001f)] public float _Accuracy;

    [Header("Directional Light")]
    public Color _mainColor;
    public Transform _directionLight;
    public Color _lightColor;
    public float _lightIntensity;

    [Header("Shadow")]
    [Range(0, 10)] public float _shadowIntensity;
    [Range(0, 100)] public float _shadowPenumbra;
    public Vector2 _shadowDistance;

    [Header("Ambient Occlusion")]
    [Range(0, 1)] public float _AoStepsize;
    [Range(0, 1)] public float _AoInstensity;
    public int _AoIterations;

    [Header("Signed Distance Field")]
    public Vector4 _sphere;
    public float _sphereSmooth;
    public float _degreeRotate;

    [Header("Reflection")]
    [Range(0, 5)] public int _ReflectionCount;
    [Range(0, 1)] public float _ReflectionIntensity;
    [Range(0, 1)] public float _ReflectionAttenuation;
    [Range(0, 1)] public float _EnvRefIntensity;
    public Cubemap _ReflectionCube;

    [Header("box sphere sdf")]
    public Vector4 _sphere1;
    public Vector4 _sphere2;
    public Vector4 _box1;
    public float _box1round;

    public float _boxSphereSmooth;
    public float _sphereIntersectSmooth;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_raymarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _raymarchMaterial.SetFloat("_maxDistance", _maxDistance);
        _raymarchMaterial.SetInt("_maxIteration", _maxIteration);
        _raymarchMaterial.SetFloat("_Accuracy", _Accuracy);
        _raymarchMaterial.SetMatrix("_CamFrustum", CamFrustum(_camera));
        _raymarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);

        _raymarchMaterial.SetVector("_lightDir", _directionLight ? _directionLight.forward : Vector3.down);
        _raymarchMaterial.SetFloat("_lightIntensity", _lightIntensity);
        _raymarchMaterial.SetColor("_mainColor", _mainColor);
        _raymarchMaterial.SetColor("_lightColor", _lightColor);
        _raymarchMaterial.SetFloat("_shadowIntensity", _shadowIntensity);
        _raymarchMaterial.SetVector("_shadowDistance", _shadowDistance);
        _raymarchMaterial.SetFloat("_shadowPenumbra", _shadowPenumbra);

        _raymarchMaterial.SetFloat("_AoStepsize", _AoStepsize);
        _raymarchMaterial.SetFloat("_AoInstensity", _AoInstensity);
        _raymarchMaterial.SetInt("_AoIterations", _AoIterations);

        _raymarchMaterial.SetInt("_ReflectionCount", _ReflectionCount);
        _raymarchMaterial.SetFloat("_ReflectionIntensity", _ReflectionIntensity);
        _raymarchMaterial.SetFloat("_ReflectionAttenuation", _ReflectionAttenuation);
        _raymarchMaterial.SetFloat("_EnvRefIntensity", _EnvRefIntensity);
        _raymarchMaterial.SetTexture("_ReflectionCube", _ReflectionCube);

        _raymarchMaterial.SetVector("_sphere", _sphere);
        _raymarchMaterial.SetFloat("_sphereSmooth", _sphereSmooth);
        _raymarchMaterial.SetFloat("_degreeRotate", _degreeRotate); 

        _raymarchMaterial.SetVector("_sphere1", _sphere1);
        _raymarchMaterial.SetVector("_sphere2", _sphere2);
        _raymarchMaterial.SetVector("_box1", _box1);
        _raymarchMaterial.SetFloat("_box1round", _box1round);
        _raymarchMaterial.SetFloat("_boxSphereSmooth", _boxSphereSmooth);
        _raymarchMaterial.SetFloat("_sphereIntersectSmooth", _sphereIntersectSmooth);
        
        _raymarchMaterial.SetTexture("_MainTex", source);

        // to do
        RenderTexture.active = destination;
        GL.PushMatrix();
        GL.LoadOrtho();
        _raymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        //BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);
        //BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        //TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);
        //TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam){
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 goUp = Vector3.up * fov;
        Vector3 goRight = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);
        Vector3 BR = (-Vector3.forward + goRight - goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);

        return frustum;
    }
}
