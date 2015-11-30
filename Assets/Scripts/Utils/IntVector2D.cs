
public class IntVector2D {
    public int X { get; set; }
    public int Y { get; set; }

    public IntVector2D(int x, int y) {
        Set(x, y);
    }

    public void Set(int x, int y) {
        X = x;
        Y = y;
    }
}
