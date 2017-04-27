﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExternalVisualization : MonoBehaviour {
	private Dictionary<float, LinkedList<SphericalCoordinate>> pointTable;
    private LidarStorage lidarStorage;
    private ExternalPointCloud externalPointCloud;
	public GameObject pSystemGameObject, nextBtn, prevBtn, mainPanel,backBtn;
	private ParticleSystem pSystem;
	private int currentListPosition; 
	public Button nextButton,prevButton,openButton,backButton;
    public Text lapText;
    public Toggle fullCloudToggle, lapToggle;
    public TestFileBrowser fileBrowser;

	public void Start() {
		pSystemGameObject  = GameObject.Find("particlesSyst");
        nextBtn = GameObject.Find("Next");
        prevBtn = GameObject.Find("Prev");
        backBtn = GameObject.Find("BackButton");
        mainPanel = GameObject.Find("MainPanel");

        pSystem = pSystemGameObject.GetComponent<ParticleSystem>();
        nextButton = nextBtn.GetComponent<Button>();
        prevButton = prevBtn.GetComponent<Button>();
        backButton = backBtn.GetComponent<Button>();
        openButton = GameObject.Find("Open").GetComponent<Button>();
        lapText = GameObject.Find("LapText").GetComponent<Text>();
        fileBrowser = GameObject.Find("FileBrowser").GetComponent<TestFileBrowser>();
        lidarStorage = GameObject.FindGameObjectWithTag("Lidar").GetComponent<LidarStorage>(); ;
        externalPointCloud = GetComponent<ExternalPointCloud>();

        openButton.onClick.AddListener(LoadPoints);
        SetState(State.Default);


        LidarStorage.HaveData += DataExists;
	}

    /// <summary>
    /// The different states the external visualization can be in.
    /// </summary>
    private enum State
    {
        Default, FullCloud, LapCloud
    }

    /// <summary>
    /// Sets the state of the visualization.
    /// </summary>
    private void SetState(State state)
    {
        if(state == State.Default)
        {
            currentListPosition = 0;
            prevBtn.SetActive(false);
            nextBtn.SetActive(false);
            mainPanel.SetActive(true);
            lapText.enabled = false;
            backBtn.SetActive(false);
        } else if(state == State.FullCloud)
        {
            prevBtn.SetActive(false);
            nextBtn.SetActive(false);
            lapText.enabled = false;
            mainPanel.SetActive(false);
            backBtn.SetActive(true);
            backButton.onClick.AddListener(Reset);

        }
        else
        {
            currentListPosition = 0;
            prevBtn.SetActive(true);
            nextBtn.SetActive(true);
            mainPanel.SetActive(false);
            lapText.enabled = true;
            backBtn.SetActive(true);
            nextButton.onClick.AddListener(LoadNext);
            prevButton.onClick.AddListener(LoadPrev);
            backButton.onClick.AddListener(Reset);
        }
    }
   

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            LoadNext();
        }
        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            LoadPrev();
        }
    }



	/// <summary>
	/// Opens the file dialog and loads a set of previously loaded points. 
	/// </summary>
	public void LoadPoints()
	{
        fileBrowser.SetActive(true);
    }
    private LinkedList<SphericalCoordinate> createList(Dictionary<float, LinkedList<SphericalCoordinate>> data)
    {
        LinkedList<SphericalCoordinate> newList = new LinkedList<SphericalCoordinate>();
        foreach (var entity in data)
        {
            for (LinkedListNode<SphericalCoordinate> it = entity.Value.First; it != null; it = it.Next)
            {
                newList.AddLast(it.Value);
            }
        }
        return newList;
    }

    /// <summary>
    /// Creates a single linked list filled with spherical coordinates from a data tablöe
    /// </summary>
    /// <returns></returns>
    private LinkedList<SphericalCoordinate> SquashTable(Dictionary<float, LinkedList<SphericalCoordinate>> data)
    {
        LinkedList<SphericalCoordinate> newList = new LinkedList<SphericalCoordinate>();

        foreach(var entity in data)
        {
            foreach(SphericalCoordinate s in entity.Value)
            {
                newList.AddLast(s);
            }
        }
        return newList;
    }



	/// <summary>
	/// Tells the particle system to load the next set of points. 
	/// </summary>
	public void LoadNext()
	{
        if (pointTable != null && pointTable.Count != 0) {
            if (currentListPosition + 1 < pointTable.Count) {
                currentListPosition += 1;
                ParticleSystem.Particle[] particles = CreateParticles(pointTable, currentListPosition);
                pSystem.SetParticles(particles, particles.Length);
                lapText.text = "Lap: " + currentListPosition;
            }
        } else
        {
            pointTable = lidarStorage.GetData();
        }
    }
	/// <summary>
	/// Tells the particle system to load the previous set of points. 
	/// </summary>
	public void LoadPrev()
	{
        if (pointTable != null)
        {
            if (currentListPosition - 1 >= 0)
            {
                currentListPosition -= 1;
                
                ParticleSystem.Particle[] particles = CreateParticles(pointTable, currentListPosition);
                pSystem.Clear();
                pSystem.SetParticles(particles, particles.Length);
                pSystem.Play();
                lapText.text = "Lap: " + currentListPosition;

            }
        } else
        {
            this.pointTable = lidarStorage.GetData();
        }
    }

    /// <summary>
    /// Resets the visualization to it's initial state
    /// </summary>
    private void Reset()
    {
        SetState(State.Default);
    }


	private ParticleSystem.Particle[] CreateParticles(Dictionary<float,LinkedList<SphericalCoordinate>> data, int position)
	{
		List<ParticleSystem.Particle> particleCloud = new List<ParticleSystem.Particle>();
        LinkedList<SphericalCoordinate> list = new LinkedList<SphericalCoordinate>();
        int pos = 0;
        foreach(var v in data)
        {
            if(pos == position)
            {
                list = v.Value;
                break;
            }
            pos++;
        }

		for (LinkedListNode<SphericalCoordinate> it = list.First; it != null; it = it.Next)
		{           
                ParticleSystem.Particle particle = new ParticleSystem.Particle();
                particle.position = it.Value.ToCartesian();
                if (it.Value.GetInclination() < 3)
                {
                    particle.startColor = Color.red;
                }
                else if (it.Value.GetInclination() > 3 && it.Value.GetInclination() < 7)
                {
                    particle.startColor = Color.yellow;
                }
                else
                {
                    particle.startColor = Color.green;
                }

                particle.startSize = 0.1f;
                particle.startLifetime =100f;
                particle.remainingLifetime = 100f;
                particleCloud.Add(particle);            
		}

		return particleCloud.ToArray();
	}



    private void DataExists()
    {
        this.pointTable = lidarStorage.GetData();
        if (fullCloudToggle.isOn)
        {
            SetState(State.FullCloud);
        }
        else
        {
            SetState(State.LapCloud);
            pointTable = lidarStorage.GetData();

        }

    }





	}
