using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System.Threading;


public class KinectCoordinateMapper : MonoBehaviour
{

    private KinectSensor kinectSensor;
    private MultiSourceFrameReader multiSouseFrameReader;
    private CoordinateMapper coordinateMapper;

    private FrameDescription bodyIndexFrameDescription;
    private byte[] bodyIndexData;

    private FrameDescription colorFrameDescription;
    private byte[] colorData;

    private FrameDescription depthFrameDescription;
    private ushort[] depthData;

    private Texture2D texture;
    private byte[] textureData;

    private Sprite image;
    private SpriteRenderer spriteRenderer;



    // Use this for initialization
    void Start()
    {
        //KinectセンサーOpen
        kinectSensor = KinectSensor.GetDefault();
        multiSouseFrameReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.BodyIndex | FrameSourceTypes.Color | FrameSourceTypes.Depth);
        coordinateMapper = kinectSensor.CoordinateMapper;

        //各Frameデータのバッファを作成
        bodyIndexFrameDescription = kinectSensor.BodyIndexFrameSource.FrameDescription;
        bodyIndexData = new byte[bodyIndexFrameDescription.Height * bodyIndexFrameDescription.Width];
        colorFrameDescription = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
        colorData = new byte[colorFrameDescription.Height * colorFrameDescription.Width * colorFrameDescription.BytesPerPixel];
        depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;
        depthData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];

        //テクスチャの初期化とSpriteへの登録
        texture = new Texture2D(bodyIndexFrameDescription.Width, bodyIndexFrameDescription.Height, TextureFormat.RGBA32, false);
        textureData = new byte[texture.width * texture.height * 4];

        image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = image;

        if (!kinectSensor.IsOpen)
        {
            kinectSensor.Open();
        }
    }

    //On Close Event
    void OnDestroy()
    {
        if (multiSouseFrameReader != null)
        {
            multiSouseFrameReader.Dispose();
            multiSouseFrameReader = null;
        }

        if (kinectSensor != null)
        {
            kinectSensor.Close();
            kinectSensor = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (multiSouseFrameReader != null)
        {
            var frame = multiSouseFrameReader.AcquireLatestFrame();
            if (frame != null)
            {
                using (var bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame())
                {
                    if (bodyIndexFrame != null)
                    {
                        bodyIndexFrame.CopyFrameDataToArray(bodyIndexData);
                    }
                }

                using (var colorFrame = frame.ColorFrameReference.AcquireFrame())
                {
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                    }
                }

                using (var depthFrame = frame.DepthFrameReference.AcquireFrame())
                {
                    if (depthFrame != null)
                    {
                        depthFrame.CopyFrameDataToArray(depthData);
                    }
                }
            }
        }

        //各データ間の対応を取得
        ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[bodyIndexData.Length];
        coordinateMapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        for (int i = 0; i < bodyIndexData.Length; i++)
        {
            byte r, g, b, a;
            if (bodyIndexData[i] != 255)
            {
                ColorSpacePoint point = colorSpacePoints[i];
                int colorX = (int)point.X;
                int colorY = (int)point.Y;
                int colorIndex = colorFrameDescription.Width * colorY + colorX;

                if (colorX >= 0 && colorX < colorFrameDescription.Width && colorY >= 0 && colorY < colorFrameDescription.Height)
                {
                    r = colorData[colorIndex * 4 + 0];
                    g = colorData[colorIndex * 4 + 1];
                    b = colorData[colorIndex * 4 + 2];
                    a = 255;
                }
                else
                {
                    r = g = b = 0;
                    a = 255;
                }
            }
            else
            {
                r = g = b = a = 0;
            }

            textureData[i * 4 + 0] = r;
            textureData[i * 4 + 1] = g;
            textureData[i * 4 + 2] = b;
            textureData[i * 4 + 3] = a;
        }
        texture.LoadRawTextureData(textureData);
        texture.Apply();

        spriteRenderer.sprite = image;
    }

}
