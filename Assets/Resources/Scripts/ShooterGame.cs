using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShooterGame : MonoBehaviour
{

	[SerializeField]
	private Canvas m_GameCanvas;

	[SerializeField]
	private GameObject m_Wall;

	[SerializeField]
	private Canvas m_Menu;

	[SerializeField]
	private TextMesh m_ScoreText;

	[SerializeField]
	private TextMesh m_FloatingNumbers;

	[SerializeField]
	private Image[] m_Buttons;

	[SerializeField]
	private GameObject m_GazePointer;

	[SerializeField]
	[Range( 1.0f, 2.0f )]
	private float m_SpawnStep;

	private int m_Score = 0;
	private float m_TimeDelta = 0.0f;
	private bool m_GameStarted = false;
	private Vector3 m_FloatingNumPosition = Vector3.zero;
	private Vector3 m_TextScale = Vector3.zero;
	private Color m_NormalColor;

	// Use this for initialization
	void Start()
	{
		m_NormalColor = m_Buttons[0].color;
	}

	// Update is called once per frame
	void Update()
	{
		m_TimeDelta += Time.deltaTime;
		if ( m_TimeDelta > m_SpawnStep && m_GameStarted )
		{
			AllButtonsOff();
			ButtonOn( m_Buttons[Random.Range( 0, 11 )] );
			m_TimeDelta = 0.0f;
		}
		m_ScoreText.text = m_Score.ToString();
	}

	public void BeginGame()
	{
		m_Wall.gameObject.SetActive( true );
		m_Menu.gameObject.SetActive( false );
		m_GameCanvas.gameObject.SetActive( true );
		m_Score = 0;

		StartCoroutine( Countdown() );
	}

	public void EndGame()
	{
		m_Wall.gameObject.SetActive( false );
		m_Menu.gameObject.SetActive( true );
		m_GameCanvas.gameObject.SetActive( false );
		m_GameStarted = false;
	}

	public void ResetGame()
	{
		m_GameStarted = false;
		AllButtonsOff();
		m_Score = 0;
		StartCoroutine( Countdown() );
	}

	public void Shoot( Image _ClickedButton )
	{
		if ( _ClickedButton.color == Color.red )
		{
			m_Score++;
			m_TimeDelta = m_SpawnStep;
			StartCoroutine( TextFade( _ClickedButton, true ) );
		}
		else
		{
			StartCoroutine( TextFade( _ClickedButton, false ) );
		}
	}

	public void GameOver()
	{
		m_GameStarted = false;
		AllButtonsOff();
	}

	private IEnumerator Countdown()
	{
		m_FloatingNumbers.color = Color.red;

		if ( m_FloatingNumPosition == Vector3.zero )
		{
			m_FloatingNumPosition = m_FloatingNumbers.transform.position;
		}
		else
		{
			m_FloatingNumbers.transform.position = m_FloatingNumPosition;
		}

		m_FloatingNumbers.transform.localScale *= 2.0f;
		m_FloatingNumbers.gameObject.SetActive( true );

		for ( int i = 5; i > 0; i-- )
		{
			m_FloatingNumbers.text = i.ToString();
			yield return new WaitForSeconds( 1f );
		}

		m_GameStarted = true;
		m_FloatingNumbers.gameObject.SetActive( false );
		m_FloatingNumbers.transform.localScale *= 1.0f / 2.0f;
		yield return null;
	}

	private IEnumerator TextFade( Image _ClickedButton, bool _Hit )
	{
		TextMesh PlusOne = ( _ClickedButton.GetComponent( "TargetButton" ) as TargetButton ).TextMesh;
		float TimePassed = 0.0f;
		PlusOne.color = Color.blue;
		Vector4 end = Color.blue;
		end.w = 0.0f;

		if ( m_TextScale == Vector3.zero )
		{
			m_TextScale = PlusOne.transform.localScale;
		}


		if ( !_Hit )
		{
			PlusOne.transform.localScale = m_TextScale * 0.5f;
			PlusOne.text = "Miss!";
		}
		else
		{
			PlusOne.transform.localScale = m_TextScale;
			PlusOne.text = "+1";
		}

		PlusOne.gameObject.SetActive( true );

		while ( TimePassed < 1.5f )
		{
			TimePassed += Time.deltaTime;
			PlusOne.color = Vector4.Lerp( PlusOne.color, end, 1.5f * Time.deltaTime );
			yield return null;
		}

		PlusOne.gameObject.SetActive( false );

		yield return null;
	}

	//private IEnumerator EndGameMessage()
	//{
	//
	//}

	private void AllButtonsOff()
	{
		for ( int i = 0; i < m_Buttons.Length; i++ )
		{
			ButtonOff( m_Buttons[i] );
		}
	}

	private void ButtonOn( Image _Image )
	{
		_Image.color = Color.red;
	}

	private void ButtonOff( Image _Image )
	{
		_Image.color = m_NormalColor;
	}


}
