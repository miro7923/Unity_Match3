//전 클래스에서 공통으로 사용되는 상수 및 구조체들을 모아놓은 Script

public class Constants
{
    public const int TIMER_MAX = 60;
    public const int TIMES_CREATEBLOCK = 100;
}

public enum PROCESS
{
    TITLE,
    READY,
    INGAME,
    SCORE,
    MESSAGE,
    SETTING
}

public enum DIRECTION
{
    NONE,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public enum BLOCKSTATUS
{
    NORMAL,
    MOVE,
}

public enum INPUTUP_RESULT
{
    IN,
    OUT,
    NOT_SELECTED
}

public struct Coordinate
{
    public int x;
    public int y;

    public Coordinate(int valueX, int valueY)
    {
        this.x = valueX;
        this.y = valueY;
    }

    public bool isValid(int iCol, int iRow)
    {
        //현재 저장된 좌표가 유효 범위 내인지 확인 
        return 0 <= this.x && 0 <= this.y && iCol > this.y && iRow > this.x;
    }
}

public struct Size
{
    public float fWidth;
    public float fHeight;

    public Size(float width, float height)
    {
        this.fWidth = width;
        this.fHeight = height;
    }
}
