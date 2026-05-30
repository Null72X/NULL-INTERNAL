using ImGuiNET;
using System.Numerics;
using System.Runtime.InteropServices;
using static Client.WinAPI;
using System.Linq;

namespace Client
{
    internal class ESP : ClickableTransparentOverlay.Overlay
    {
        IntPtr hWnd;
        IntPtr HDPlayer;
        private const short DefaultMaxHealth = 200;

        protected override unsafe void Render()
        {
            if (!IsGameWindowActive()) return;

            var fg = ImGui.GetForegroundDrawList();
            var bg = ImGui.GetBackgroundDrawList();

            if (!Core.HaveMatrix && !Core.IsSpectating)
                return;

            CreateHandle();
            var drawList = fg;

            string windowName = "Overlay";
            if (hWnd == IntPtr.Zero)
                hWnd = FindWindow(null, windowName);

            if (HDPlayer == IntPtr.Zero)
                HDPlayer = FindWindow("BlueStacksApp", null);

            if (hWnd != IntPtr.Zero)
            {
                long extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                SetWindowLong(hWnd, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            }

            bool isInMatch = false;
            int enemyCount = 0;
            Vector3 localPos = Core.LocalMainCamera;

            foreach (var ent in Core.Entities.Values)
            {
                if (ent == null || ent.IsDead || !ent.IsKnown)
                    continue;

                isInMatch = true;

                if (ent.IsTeam == Bool.True)
                    continue;
                if (Vector3.Distance(localPos, ent.Root) <= Config.AimBotMaxDistance)
                    enemyCount++;
            }

            if (isInMatch)
            {
                if (Config.Text)
                {
                    void DrawTextWithSoftBg(ImDrawListPtr dl, Vector2 pos, string text, uint textColor)
                    {
                        Vector2 size = ImGui.CalcTextSize(text);
                        float padX = 7f;
                        float padY = 4f;
                        float rounding = 6f;

                        Vector2 bgMin = pos - new Vector2(padX, padY);
                        Vector2 bgMax = pos + size + new Vector2(padX, padY);

                        dl.AddRectFilled(bgMin, bgMax, ColorToUint32(Color.FromArgb(130, 15, 15, 15)), rounding, ImDrawFlags.RoundCornersAll);
                        dl.AddText(pos + new Vector2(1, 1), ColorToUint32(Color.Black), text);
                        dl.AddText(pos, textColor, text);
                    }

                    string title = "GAURAV ON TOP";
                    string subtitle = "DEV </> GAURAV";
                    uint accent = ColorToUint32(Color.FromArgb(255, 235, 60, 60));
                    uint white = ColorToUint32(Color.FromArgb(255, 230, 230, 230));

                    float baseY = 65f;
                    float centerX = Core.Width / 2f;
                    Vector2 titleSize = ImGui.CalcTextSize(title);
                    Vector2 subSize = ImGui.CalcTextSize(subtitle);
                    Vector2 titlePos = new(centerX - titleSize.X / 2f, baseY);
                    Vector2 subPos = new(centerX - subSize.X / 2f, baseY + 22f);

                    DrawTextWithSoftBg(drawList, titlePos, title, accent);
                    DrawTextWithSoftBg(drawList, subPos, subtitle, white);

                    string rangeText = $"ENEMIES IN RANGE : {enemyCount}";
                    Vector2 rangeSize = ImGui.CalcTextSize(rangeText);
                    Vector2 rangePos = new(centerX - rangeSize.X / 2f, baseY + 45f);

                    uint rangeColor = white;
                    if (enemyCount > 0)
                    {
                        float pulse = (float)(Math.Sin(ImGui.GetTime() * 6f) * 0.5f + 0.5f);
                        rangeColor = ColorToUint32(Color.FromArgb(255, (int)(235 + pulse * 20), (int)(60 - pulse * 10), (int)(60 - pulse * 10)));
                    }

                    DrawTextWithSoftBg(drawList, rangePos, rangeText, rangeColor);
                }

                if (Config.MiniMap)
                {
                    DrawMiniMap();
                }
            }

            var entities = Core.Entities.Values.ToArray();
            foreach (var entity in entities)
            {
                if (entity.IsDead || !entity.IsKnown)
                    continue;

                var dist = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (dist > 150f)
                    continue;

                var headScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                var bottomScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Root, Core.Width, Core.Height);

                if (headScreenPos.X < 1 || headScreenPos.Y < 1) continue;
                if (bottomScreenPos.X < 1 || bottomScreenPos.Y < 1) continue;

                float CornerHeight = Math.Abs(headScreenPos.Y - bottomScreenPos.Y);
                float CornerWidth = (float)(CornerHeight * 0.65);


                if (Config.ESPLine)
                {
                    if (headScreenPos.X < 0 || headScreenPos.X > Core.Width || headScreenPos.Y < 0 || headScreenPos.Y > Core.Height)
                        continue;

                    uint lineColor = entity.IsKnocked ? ColorToUint32(Color.Red) : ColorToUint32(Config.ESPLineColor);
                    var bgDrawList = bg;

                    bgDrawList.AddLine(new Vector2(Core.Width / 2f, 25f), headScreenPos, lineColor, 1);
                    bgDrawList.AddCircleFilled(new Vector2(Core.Width / 2f, 25f), 2, ColorToUint32(Color.Red));
                }

                if (Config.ESPFillBox)
                {
                    Color chosen = Config.ESPFillBoxColor;
                    Color topColor = Color.FromArgb(0, chosen.R, chosen.G, chosen.B);
                    Color bottomColor = Color.FromArgb(255, chosen.R, chosen.G, chosen.B);

                    Vector2 topLeft = new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y);
                    Vector2 bottomRight = new Vector2(headScreenPos.X + (CornerWidth / 2), headScreenPos.Y + CornerHeight);

                    drawList.AddRectFilledMultiColor(topLeft, bottomRight, ColorToUint32(topColor), ColorToUint32(topColor), ColorToUint32(bottomColor), ColorToUint32(bottomColor));
                }

                if (Config.ESPBox)
                {
                    uint boxColor = entity.IsKnocked ? ColorToUint32(Color.Red) : ColorToUint32(Config.ESPBoxColor);
                    Draw2dBox(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y, CornerWidth, CornerHeight, boxColor, 1f);
                }

                if (Config.ESPCornerbox)
                {
                    uint boxColor = entity.IsKnocked ? ColorToUint32(Color.Red) : ColorToUint32(Config.ESPCornerboxColor);
                    DrawCorneredBox(new Vector2(headScreenPos.X - (CornerWidth / 2), headScreenPos.Y), CornerWidth, CornerHeight, boxColor, 1f);
                }

                var nameText = string.IsNullOrWhiteSpace(entity.Name) ? "BOT" : entity.Name.ToUpperInvariant();
                var distanceText = $"{MathF.Round(dist)}m";

                Vector2 nameSize = ImGui.CalcTextSize(nameText);
                Vector2 distanceSize = ImGui.CalcTextSize(distanceText);

                void DrawBoldText(ImDrawListPtr dl, Vector2 pos, uint color, string text)
                {
                    uint shadow = ColorToUint32(Color.Black);
                    dl.AddText(pos + new Vector2(1, 0), shadow, text);
                    dl.AddText(pos + new Vector2(-1, 0), shadow, text);
                    dl.AddText(pos + new Vector2(0, 1), shadow, text);
                    dl.AddText(pos + new Vector2(0, -1), shadow, text);
                    dl.AddText(pos, color, text);
                }

                float spacing = 2f;
                float singleAboveY = headScreenPos.Y - 14f;
                float stackedAboveY = headScreenPos.Y - (distanceSize.Y + nameSize.Y + 6f);

                if (Config.ESPDistance && Config.ESPName && dist <= 100f)
                {
                    float currentY = stackedAboveY;
                    Vector2 distPos = new(headScreenPos.X - distanceSize.X / 2f, currentY);
                    DrawBoldText(drawList, distPos, ColorToUint32(Color.White), distanceText);
                    currentY += distanceSize.Y + spacing;
                    Vector2 namePos = new(headScreenPos.X - nameSize.X / 2f, currentY);
                    DrawBoldText(drawList, namePos, ColorToUint32(Color.White), nameText);
                }
                else if (Config.ESPDistance && dist <= 100f)
                {
                    Vector2 distPos = new(headScreenPos.X - distanceSize.X / 2f, singleAboveY);
                    DrawBoldText(drawList, distPos, ColorToUint32(Color.White), distanceText);
                }
                else if (Config.ESPName && dist <= 100f)
                {
                    Vector2 namePos = new(headScreenPos.X - nameSize.X / 2f, singleAboveY);
                    DrawBoldText(drawList, namePos, ColorToUint32(Color.White), nameText);
                }

                // ----- Health Bar & Health Text (corrected positioning) -----
                float boxX = headScreenPos.X - (CornerWidth / 2);
                float boxY = headScreenPos.Y;
                float boxHeight = CornerHeight;

                const float healthBarWidth = 4f;
                const float barPadding = 2f;
                float barX = boxX - healthBarWidth - barPadding;
                float barY = boxY;

                // Draw health bar if enabled
                if (Config.ESPHealth)
                {
                    DrawHealthBar(entity.Health, DefaultMaxHealth, barX, barY, boxHeight);
                }

                float healthTextBottomY = -1f;

                // Draw health text
                if (Config.ESPHealthText && dist <= 100f)
                {
                    float healthPercentage = Math.Clamp((float)entity.Health / DefaultMaxHealth, 0f, 1f);
                    Vector2 healthTextPos;

                    if (Config.ESPHealth)
                    {
                        // Both enabled → text below the health bar
                        float healthBarHeight = boxHeight * healthPercentage;
                        healthTextPos = new Vector2(barX + (healthBarWidth / 2f), barY + healthBarHeight + 4f);
                    }
                    else
                    {
                        // Only text enabled → text below the ESP box
                        healthTextPos = new Vector2(boxX + (CornerWidth / 2f), boxY + boxHeight + 4f);
                    }

                    string hpText = $"{entity.Health} HP";
                    Vector2 textSize = ImGui.CalcTextSize(hpText);
                    healthTextPos.X -= textSize.X / 2f; // center horizontally

                    uint shadow = ColorToUint32(Color.Black);
                    uint white = ColorToUint32(Color.White);

                    drawList.AddText(healthTextPos + new Vector2(1, 0), shadow, hpText);
                    drawList.AddText(healthTextPos + new Vector2(-1, 0), shadow, hpText);
                    drawList.AddText(healthTextPos + new Vector2(0, 1), shadow, hpText);
                    drawList.AddText(healthTextPos + new Vector2(0, -1), shadow, hpText);
                    drawList.AddText(healthTextPos, white, hpText);

                    healthTextBottomY = healthTextPos.Y + textSize.Y;
                }

                if (Config.ESPLevel && dist <= 100f)
                {
                    string levelText = $"Lv.{entity.Level}";
                    Vector2 levelSize = ImGui.CalcTextSize(levelText);

                    float levelY = singleAboveY;

                    // If name exists, move level upward
                    if (Config.ESPName)
                    {
                        levelY -= nameSize.Y + 2f;
                    }

                    // If distance exists too, move even more upward
                    if (Config.ESPDistance)
                    {
                        levelY -= distanceSize.Y + 2f;
                    }

                    Vector2 levelPos = new Vector2(
                        headScreenPos.X - levelSize.X / 2f,
                        levelY
                    );

                    uint shadow = ColorToUint32(Color.Black);
                    uint white = ColorToUint32(Color.White);

                    drawList.AddText(levelPos + new Vector2(1, 0), shadow, levelText);
                    drawList.AddText(levelPos + new Vector2(-1, 0), shadow, levelText);
                    drawList.AddText(levelPos + new Vector2(0, 1), shadow, levelText);
                    drawList.AddText(levelPos + new Vector2(0, -1), shadow, levelText);
                    drawList.AddText(levelPos, white, levelText);
                }

                if (Config.ESPSkeleton && dist <= 100f)
                {
                    DrawSkeleton(entity);
                }

                if (Config.ESPWeapon && dist <= 100f)
                {
                    string weaponName =
                        WeaponIndex.GetWeaponName(entity.WeaponID);

                    Vector2 textSize =
                        ImGui.CalcTextSize(weaponName);

                    Vector2 weaponPos;

                    if (Config.ESPHealthText && dist <= 100f)
                    {
                        float weaponY;

                        if (healthTextBottomY > 0)
                        {
                            weaponY = healthTextBottomY + 4f;
                        }
                        else
                        {
                            weaponY = bottomScreenPos.Y + 22f;
                        }

                        weaponPos = new Vector2(
                            headScreenPos.X - textSize.X / 2f,
                            weaponY
                        );
                    }
                    else
                    {
                        weaponPos = new Vector2(
                            headScreenPos.X - textSize.X / 2f,
                            bottomScreenPos.Y + 4f
                        );
                    }

                    uint shadow = ColorToUint32(Color.Black);
                    uint white = ColorToUint32(Color.White);

                    drawList.AddText(
                        weaponPos + new Vector2(1, 0),
                        shadow,
                        weaponName
                    );

                    drawList.AddText(
                        weaponPos + new Vector2(-1, 0),
                        shadow,
                        weaponName
                    );

                    drawList.AddText(
                        weaponPos + new Vector2(0, 1),
                        shadow,
                        weaponName
                    );

                    drawList.AddText(
                        weaponPos + new Vector2(0, -1),
                        shadow,
                        weaponName
                    );

                    drawList.AddText(
                        weaponPos,
                        white,
                        weaponName
                    );
                }
            }

            if (Config.AimFov && isInMatch)
            {
                uint colorUint32 = ColorToUint32(Config.ESPLineColor);
                DrawCircleFov(new Vector2(Core.Width / 2f, Core.Height / 2f), Config.AimFovCircle, colorUint32);
            }

            if (Config.FovNearMe && isInMatch)
            {
                DrawEnemyNear();
            }
        }
        
        private bool IsGameWindowActive()
        {
            IntPtr foregroundWindow = WinAPI.GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero) return false;

            if (Core.Handle == IntPtr.Zero) return false;

            if (!GetWindowRect(Core.Handle, out RECT gameRect)) return false;

            if (gameRect.Right - gameRect.Left < 100 || gameRect.Bottom - gameRect.Top < 100)
                return false;

            uint fgProcessId = 0;
            uint gameProcessId = 0;
            WinAPI.GetWindowThreadProcessId(foregroundWindow, out fgProcessId);
            WinAPI.GetWindowThreadProcessId(Core.Handle, out gameProcessId);

            if (fgProcessId == gameProcessId && fgProcessId != 0)
                return true;

            return foregroundWindow == Core.Handle;
        }

        private float GetCameraYaw()
        {
            Matrix4x4 m = Core.CameraMatrix;
            return MathF.Atan2(m.M31, m.M33);
        }

        private void DrawEnemyNear()
        {
            var draw = ImGui.GetBackgroundDrawList();
            Vector2 center = new Vector2(Core.Width / 2f, Core.Height / 2f);
            float warnDistance = 50;
            float radius = MathF.Min(Core.Width, Core.Height) * 0.24f;
            float cameraYaw = -GetCameraYaw();
            float cosYaw = MathF.Cos(cameraYaw);
            float sinYaw = MathF.Sin(cameraYaw);

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead)
                    continue;

                float distance = Vector3.Distance(Core.LocalMainCamera, entity.Head);
                if (distance > warnDistance)
                    continue;

                Vector3 relative = entity.Head - Core.LocalMainCamera;
                float rotatedX = relative.X * cosYaw - relative.Z * sinYaw;
                float rotatedY = relative.X * sinYaw + relative.Z * cosYaw;
                Vector2 dir = Vector2.Normalize(new Vector2(rotatedX, -rotatedY));

                if (float.IsNaN(dir.X) || float.IsNaN(dir.Y))
                    continue;

                float danger = 1f - (distance / warnDistance);
                float angle = MathF.Atan2(dir.Y, dir.X);
                float arcSize = 0.15f + danger * 0.25f;

                float thickness = 2.5f + danger * 1.2f;

                Vector4 color = new Vector4(1f, 1f, 1f, 0.45f * danger);
                if (entity.IsKnocked)
                    color = new Vector4(1f, 0f, 0f, 0.45f * danger);

                draw.PathClear();
                draw.PathArcTo(center, radius, angle - arcSize, angle + arcSize, 32);
                draw.PathStroke(ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.None, thickness);

                draw.PathClear();
                draw.PathArcTo(center, radius, angle - arcSize * 0.7f, angle + arcSize * 0.7f, 32);
                draw.PathStroke(ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, 0.18f * danger)), ImDrawFlags.None, 1.5f);

                float textOffset = 15f;
                Vector2 textPos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (radius + textOffset);
                string distText = $"{distance:F1}m";
                float textAlpha = Math.Clamp(danger, 0.35f, 1f);
                float fontSize = 15f + danger * 4f;
                draw.AddText(null, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, textAlpha)), distText);
            }
        }

        public void DrawMiniMap()
        {
            bool isInMatch = false;
            foreach (var ent in Core.Entities.Values)
            {
                if (ent != null && !ent.IsDead && ent.IsKnown)
                {
                    isInMatch = true;
                    break;
                }
            }

            if (!isInMatch) return;

            int enemyCount = 0;
            Vector3 localPos = Core.LocalMainCamera;

            foreach (var ent in Core.Entities.Values)
            {
                if (ent == null || ent.IsDead || !ent.IsKnown)
                    continue;
                if (ent.IsTeam == Bool.True)
                    continue;
                if (Vector3.Distance(localPos, ent.Root) <= Config.AimBotMaxDistance)
                    enemyCount++;
            }

            if (enemyCount > 0)
            {
                var drawList = ImGui.GetForegroundDrawList();

                const float mapRadius = 120;
                const float zoom = 1;
                const float margin = 50;

                Vector2 mapCenter = new Vector2(margin + mapRadius, Core.Height - margin - mapRadius);

                drawList.AddCircleFilled(mapCenter, mapRadius, ColorToUint32(Color.FromArgb(180, 30, 30, 35)), 64);
                drawList.AddCircle(mapCenter, mapRadius, ColorToUint32(Color.FromArgb(255, 220, 220, 220)), 64, 1.8f);
                drawList.AddCircle(mapCenter, mapRadius * 0.66f, ColorToUint32(Color.FromArgb(150, 255, 255, 255)), 64, 1f);
                drawList.AddCircle(mapCenter, mapRadius * 0.33f, ColorToUint32(Color.FromArgb(150, 255, 255, 255)), 64, 1f);
                drawList.AddCircleFilled(mapCenter, 5f, ColorToUint32(Color.LimeGreen), 32);

                Matrix4x4 m = Core.CameraMatrix;
                float yawRad = MathF.Atan2(m.M31, m.M33);
                yawRad = -yawRad;

                foreach (var entity in Core.Entities.Values)
                {
                    if (entity.IsDead || !entity.IsKnown || entity.IsTeam == Bool.True)
                        continue;

                    float distance = Vector3.Distance(Core.LocalMainCamera, entity.Root);
                    if (distance > Config.AimBotMaxDistance)
                        continue;

                    Vector3 diff = entity.Root - Core.LocalMainCamera;
                    float dx = diff.X / zoom;
                    float dy = diff.Z / zoom;

                    float cos = MathF.Cos(yawRad);
                    float sin = MathF.Sin(yawRad);

                    float rotatedX = dx * cos - dy * sin;
                    float rotatedY = dx * sin + dy * cos;
                    rotatedY = -rotatedY;

                    Vector2 pos = mapCenter + new Vector2(rotatedX, rotatedY);

                    if (Vector2.Distance(pos, mapCenter) <= mapRadius - 4f)
                    {
                        uint dotColor = entity.IsKnocked ? ColorToUint32(Color.Orange) : ColorToUint32(Color.FromArgb(255, 255, 80, 80));
                        for (int i = 0; i < 3; i++)
                        {
                            float alpha = 1.0f - (i * 0.3f);
                            uint glowColor = AdjustAlpha(dotColor, alpha * 0.5f);
                            drawList.AddCircleFilled(pos, 4f - i, glowColor, 16);
                        }
                        drawList.AddCircleFilled(pos, 3f, dotColor, 16);
                    }
                }
            }
        }

        public void Draw2dBox(float X, float Y, float W, float H, uint color, float thickness)
        {
            var vList = ImGui.GetForegroundDrawList();
            Vector2 topLeft = new Vector2(X, Y);
            Vector2 topRight = new Vector2(X + W, Y);
            Vector2 bottomRight = new Vector2(X + W, Y + H);
            Vector2 bottomLeft = new Vector2(X, Y + H);

            vList.AddLine(topLeft, topRight, color, thickness);
            vList.AddLine(topRight, bottomRight, color, thickness);
            vList.AddLine(bottomRight, bottomLeft, color, thickness);
            vList.AddLine(bottomLeft, topLeft, color, thickness);
        }

        private void DrawSkeleton(Entity entity)
        {
            var drawList = ImGui.GetForegroundDrawList();
            uint lineColor = ColorToUint32(Config.ESPSkeletonColor);
            uint circleColor = ColorToUint32(Color.Red);

            var HeadScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
            var BreastScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Breast, Core.Width, Core.Height);
            var HipScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Hip, Core.Width, Core.Height);
            var LeftBicepsScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftBiceps, Core.Width, Core.Height);
            var RightBicepsScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightBiceps, Core.Width, Core.Height);
            var LeftWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftWristJoint, Core.Width, Core.Height);
            var RightWristJointScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightWristJoint, Core.Width, Core.Height);
            var LeftShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftShoulder, Core.Width, Core.Height);
            var RightShoulderScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightShoulder, Core.Width, Core.Height);
            var LeftLegScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftLeg, Core.Width, Core.Height);
            var RightLegScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightLeg, Core.Width, Core.Height);
            var LeftFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.LeftFoot, Core.Width, Core.Height);
            var RightFootScreenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.RightFoot, Core.Width, Core.Height);

            if (!entity.IsKnocked)
            {
                DrawLine(HeadScreenPos, BreastScreenPos, lineColor, 1f);
                DrawLine(BreastScreenPos, HipScreenPos, lineColor, 1f);
                DrawLine(LeftShoulderScreenPos, RightShoulderScreenPos, lineColor, 1f);
                DrawLine(BreastScreenPos, LeftShoulderScreenPos, lineColor, 1f);
                DrawLine(LeftShoulderScreenPos, LeftWristJointScreenPos, lineColor, 1f);
                DrawLine(LeftWristJointScreenPos, LeftBicepsScreenPos, lineColor, 1f);
                DrawLine(BreastScreenPos, RightShoulderScreenPos, lineColor, 1f);
                DrawLine(RightShoulderScreenPos, RightWristJointScreenPos, lineColor, 1f);
                DrawLine(RightWristJointScreenPos, RightBicepsScreenPos, lineColor, 1f);
                DrawLine(HipScreenPos, LeftLegScreenPos, lineColor, 1f);
                DrawLine(HipScreenPos, RightLegScreenPos, lineColor, 1f);
                DrawLine(LeftLegScreenPos, LeftFootScreenPos, lineColor, 1f);
                DrawLine(RightLegScreenPos, RightFootScreenPos, lineColor, 1f);
            }

            if (HeadScreenPos.X > 0 && HeadScreenPos.Y > 0)
            {
                float distance = entity.Distance;
                float baseRadius = 50.0f;
                float circleRadius = baseRadius / distance;
                drawList.AddCircle(HeadScreenPos, circleRadius, circleColor, 20, 1.5f);
            }
        }

        static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f));
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        const uint WDA_NONE = 0x00000000;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        void CreateHandle()
        {
            RECT rect;
            GetWindowRect(Core.Handle, out rect);
            int x = rect.Left;
            int y = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            ImGui.SetWindowSize(new Vector2(width, height));
            ImGui.SetWindowPos(new Vector2(x, y));
            Size = new Size(width, height);
            Position = new Point(x, y);

            Core.Width = width;
            Core.Height = height;
            if (Config.StreamMode)
                SetWindowDisplayAffinity(hWnd, WDA_EXCLUDEFROMCAPTURE);
            else
                SetWindowDisplayAffinity(hWnd, WDA_NONE);
        }

        public static uint AdjustAlpha(uint color, float alpha)
        {
            byte r = (byte)(color >> 24);
            byte g = (byte)(color >> 16);
            byte b = (byte)(color >> 8);
            byte a = (byte)(color);

            a = (byte)Math.Clamp(alpha * 255f, 0, 255);
            float tintFactor = 1.1f;
            r = (byte)Math.Clamp(r * tintFactor, 0, 255);
            g = (byte)Math.Clamp(g * tintFactor, 0, 255);
            float brightnessFactor = 1.05f;
            r = (byte)Math.Clamp(r * brightnessFactor, 0, 255);
            g = (byte)Math.Clamp(g * brightnessFactor, 0, 255);
            b = (byte)Math.Clamp(b * brightnessFactor, 0, 255);

            return (uint)((r << 24) | (g << 16) | (b << 8) | a);
        }

        public void DrawLine(Vector2 start, Vector2 end, uint color, float thickness)
        {
            ImGui.GetBackgroundDrawList().AddLine(start, end, color, thickness);
        }

        public void DrawCircleFov(Vector2 center, float radius, uint color)
        {
            ImGui.GetBackgroundDrawList().AddCircle(center, radius, color, 200, 1.0f);
        }

        public void DrawHealthBar(short health, short maxHealth, float X, float Y, float boxHeight)
        {
            var vList = ImGui.GetForegroundDrawList();
            if (maxHealth <= 0) maxHealth = 200;
            float healthPercentage = Math.Clamp((float)health / maxHealth, 0f, 1f);

            float barWidth = 3f;
            float healthHeight = boxHeight * healthPercentage;

            Color topColor = Color.FromArgb(255, 0, 255, 0);
            Color midColor1 = Color.FromArgb(255, 255, 255, 0);
            Color midColor2 = Color.FromArgb(255, 255, 140, 0);
            Color bottomColor = Color.FromArgb(255, 255, 0, 0);

            vList.AddRectFilled(new Vector2(X, Y), new Vector2(X + barWidth, Y + boxHeight), ColorToUint32(Color.FromArgb(255, 50, 50, 50)));

            int segments = 10;
            float segmentHeight = boxHeight / segments;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Color lerpedColor;
                if (t < 0.33f)
                    lerpedColor = LerpColor(topColor, midColor1, t / 0.33f);
                else if (t < 0.66f)
                    lerpedColor = LerpColor(midColor1, midColor2, (t - 0.33f) / 0.33f);
                else
                    lerpedColor = LerpColor(midColor2, bottomColor, (t - 0.66f) / 0.34f);

                float segmentTop = Y + i * segmentHeight;
                float segmentBottom = segmentTop + segmentHeight;

                if (segmentBottom > Y + boxHeight - healthHeight)
                {
                    vList.AddRectFilled(new Vector2(X, segmentTop), new Vector2(X + barWidth, segmentBottom), ColorToUint32(lerpedColor));
                }
            }

            vList.AddRect(new Vector2(X, Y), new Vector2(X + barWidth, Y + boxHeight), ColorToUint32(Color.Black), 1.0f);
        }

        private Color LerpColor(Color color1, Color color2, float t)
        {
            return Color.FromArgb(255,
                (int)(color1.R + (color2.R - color1.R) * t),
                (int)(color1.G + (color2.G - color1.G) * t),
                (int)(color1.B + (color2.B - color1.B) * t));
        }

        public void DrawCorneredBox(Vector2 topLeft, float width, float height, uint color, float thickness)
        {
            var vList = ImGui.GetBackgroundDrawList();
            float cornerW = width / 3f;
            float cornerH = height / 3f;

            vList.AddLine(topLeft, new Vector2(topLeft.X + cornerW, topLeft.Y), color, thickness);
            vList.AddLine(topLeft, new Vector2(topLeft.X, topLeft.Y + cornerH), color, thickness);

            Vector2 topRight = new Vector2(topLeft.X + width, topLeft.Y);
            vList.AddLine(topRight, new Vector2(topRight.X - cornerW, topRight.Y), color, thickness);
            vList.AddLine(topRight, new Vector2(topRight.X, topRight.Y + cornerH), color, thickness);

            Vector2 bottomLeft = new Vector2(topLeft.X, topLeft.Y + height);
            vList.AddLine(bottomLeft, new Vector2(bottomLeft.X + cornerW, bottomLeft.Y), color, thickness);
            vList.AddLine(bottomLeft, new Vector2(bottomLeft.X, bottomLeft.Y - cornerH), color, thickness);

            Vector2 bottomRight = new Vector2(topLeft.X + width, topLeft.Y + height);
            vList.AddLine(bottomRight, new Vector2(bottomRight.X - cornerW, bottomRight.Y), color, thickness);
            vList.AddLine(bottomRight, new Vector2(bottomRight.X, bottomRight.Y - cornerH), color, thickness);
        }
    }
}