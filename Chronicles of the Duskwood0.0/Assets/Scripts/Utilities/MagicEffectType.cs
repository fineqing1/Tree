public enum MagicEffectType { Flourish, Wither }

public interface IMagicInteractable
{
    void ApplyMagic(MagicEffectType type);
}