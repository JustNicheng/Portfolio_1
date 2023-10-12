using UnityEngine;

public class LinkOldMovieMaterial : MonoBehaviour
{
    #region 載入與更新
    [SerializeField, Header("連結的Material")]
    private Material _linkMaterial;
    [SerializeField, Header("划痕")]
    private ComputeShader _scratchesShader;
    [SerializeField, Header("汙點")]
    private ComputeShader _dustShader;
    [SerializeField, Header("外圈陰影")]
    private ComputeShader _outermostShader;
    RenderTexture[] _allRenderTex;
    private void Start()
    {
        _allRenderTex = new RenderTexture[] { new RenderTexture(1024, 1024, 24), new RenderTexture(1024, 1024, 24), new RenderTexture(1024, 1024, 24) };
        LinkAllMaterial();
        _nowTimePerAni = 1.0f / theFPS;
    }
    [SerializeField,Header("老電影效果閃爍FPS")]
    private float theFPS;
    private float _nowTimePerAni;
    void Update()
    {
        _nowTimePerAni -= Time.deltaTime;
        if (_nowTimePerAni <= 0)
        {
            _nowTimePerAni += 1.0f / theFPS;
            UpdateTex();
        }
    }
    void LinkAllMaterial()
    {
        for (int i = 0; i < _allRenderTex.Length; i++)
        {
            _allRenderTex[i].enableRandomWrite = true;
            _allRenderTex[i].Create();
            switch (i)
            {
                case 0:
                    _linkMaterial.SetTexture("_ScratchTex", _allRenderTex[i]);
                    ScratchTextureUpdate(ref _allRenderTex[i]);
                    break;
                case 1:
                    _linkMaterial.SetTexture("_DustTex", _allRenderTex[i]);
                    DustTextureUpdate(ref _allRenderTex[i]);
                    break;
                case 2:
                    _linkMaterial.SetTexture("_OutermostShader", _allRenderTex[i]);
                    OutermostTextureUpdate(ref _allRenderTex[i]);
                    break;
            }


        }
    }
    void UpdateTex()
    {
        for (int i = 0; i < _allRenderTex.Length; i++)
        {
            switch (i)
            {
                case 0:
                    ScratchTextureUpdate(ref _allRenderTex[i]);
                    break;
                case 1:
                    DustTextureUpdate(ref _allRenderTex[i]);
                    break;
                case 2:
                    OutermostTextureUpdate(ref _allRenderTex[i]);
                    break;
            }


        }
    }
    #endregion
    #region Scratch
    [SerializeField, Header("劃痕寬上限"), Range(0.1f, 30f)]
    float scratchWidthMax;
    [SerializeField, Header("劃痕數量上限"), Range(1, 100)]
    int scratchCountMax;
    void ScratchTextureUpdate(ref RenderTexture _rt)
    {

        int kernelIndex = _scratchesShader.FindKernel("CSMain");
        int randomLen = Random.Range(1, scratchCountMax);//1~30條劃痕
        _scratchesShader.SetInt("posArrayaLength", randomLen);
        ComputeBuffer _uniformArray = new ComputeBuffer(randomLen, sizeof(float) * 3);
        _uniformArray.SetData(RandomScratch(randomLen));
        _scratchesShader.SetBuffer(kernelIndex, "RandomPosArray", _uniformArray);
        _scratchesShader.SetTexture(kernelIndex, "ScratchesTex", _rt);
        _scratchesShader.Dispatch(kernelIndex, 1024 / 8, 1024 / 8, 1);
        _uniformArray.Release();
    }
    Vector3[] RandomScratch(int randomLen)
    {
        Vector3[] _randomScratches = new Vector3[randomLen];
        for (int i = 0; i < randomLen; i++)
        {
            _randomScratches[i] = new Vector3(Random.Range(.0f, 1024f),//X座標
                                              0,//不需要Y座標
                                              Random.Range(.0f, scratchWidthMax));//寬度
        }
        return _randomScratches;
    }
    #endregion
    #region Dust
    [SerializeField, Header("汙點半徑上限"), Range(0.1f, 30f)]
    float dustRadiusMax;
    [SerializeField, Header("汙點數量上限"), Range(1, 100)]
    int dustCountMax;
    void DustTextureUpdate(ref RenderTexture _rt)
    {
        int kernelIndex = _dustShader.FindKernel("CSMain");
        int randomLen = Random.Range(1, dustCountMax);//隨機數量點點
        _dustShader.SetInt("posArrayaLength", randomLen);
        ComputeBuffer _uniformArray = new ComputeBuffer(randomLen, sizeof(float) * 3);
        _uniformArray.SetData(RandomDust(randomLen));
        _dustShader.SetBuffer(kernelIndex, "RandomPosArray", _uniformArray);
        _dustShader.SetTexture(kernelIndex, "DustTex", _rt);
        _dustShader.Dispatch(kernelIndex, 1024 / 8, 1024 / 8, 1);
        _uniformArray.Release();
    }
    Vector3[] RandomDust(int randomLen)
    {
        Vector3[] _randomScratches = new Vector3[randomLen];
        for (int i = 0; i < randomLen; i++)
        {
            _randomScratches[i] = new Vector3(Random.Range(.0f, 1024f),//X座標
                                              Random.Range(.0f, 1024f),//Y座標
                                              Random.Range(.0f, dustRadiusMax));//半徑
        }
        return _randomScratches;
    }
    #endregion
    #region Outermost
    [SerializeField, Header("最外圈的範圍")]
    Vector2 blackWidthRange;
    [SerializeField, Header("最外圈到中間空白之間過度的範圍")]
    Vector2 gradientWidthRange;

    void OutermostTextureUpdate(ref RenderTexture _rt)
    {
        int kernelIndex = _outermostShader.FindKernel("CSMain");
        int blackWidth = (int)Random.Range(blackWidthRange.x, blackWidthRange.y);
        int gradientWidth = (int)Random.Range(gradientWidthRange.x, gradientWidthRange.y);
        _outermostShader.SetInt("BlackWidth", blackWidth);
        _outermostShader.SetInt("GradientWidth", gradientWidth);
        _outermostShader.SetTexture(kernelIndex, "OutermostTex", _rt);
        _outermostShader.Dispatch(kernelIndex, 1024 / 8, 1024 / 8, 1);
    }
    #endregion
}
