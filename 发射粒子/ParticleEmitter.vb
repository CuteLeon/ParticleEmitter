Imports System.Drawing.Imaging

Public Class ParticleEmitter
    Dim DefaultIntervalTime As Integer = 20     '默认更新时间间隔(单位：毫秒)
    Dim DefaultAlpha As Integer = 255             '默认起始时粒子透明度
    Dim DefaultDiameter As Integer = 20          '默认粒子直径
    Dim Gravity As Single = 3.0F                        '默认重力加速度
    Dim VelocityRange() As Integer = {500, 1000} '速度取值范围
    Dim AngleRange() As Integer = {-45, 45}         '角度取值范围
    Dim LifeRange() As Integer = {400, 800}          '生命长度取值范围(单位：毫秒)
    Dim Random As New Random                        '随机数生成器
    Dim ParticleBitmap As Bitmap                         '粒子位图
    Dim ParticleGraphics As Graphics                    '粒子画笔

    Private Structure Particle
        '粒子的坐标、速度、透明度、颜色、直径、剩余生命、总生命长度
        Dim X As Integer
        Dim Y As Integer
        Dim XVelocity As Integer
        Dim YVelocity As Integer
        Dim Alpha As Single
        Dim Color As Color
        Dim Diameter As Integer
        Dim ResidualLife As Integer
        Dim LifeLength As Integer
    End Structure

    Dim Particles As New ArrayList      '记录存活粒子的数组

    '更新单个粒子状态（传入的是粒子的内存地址）
    Private Sub UpdateParticle(ByRef Particle As Particle, ByVal IntervalTime As Integer)
        With Particle
            '根据速度计算坐标
            .X += (.XVelocity * IntervalTime / 1000.0F)
            .Y += (.YVelocity * IntervalTime / 1000.0F)
            '根据重力加速度计算Y轴速度
            .YVelocity += Gravity * IntervalTime
            '根据剩余生命比计算透明度和粒子直径
            .Alpha = DefaultAlpha * (.ResidualLife / .LifeLength) * 0.8 + 51
            .Diameter = DefaultDiameter * (.ResidualLife / .LifeLength) * 0.6 + 4
            '生命流逝
            .ResidualLife -= IntervalTime
        End With
    End Sub

    '发射单颗粒子
    Private Sub EmitSingleParticle(ByVal X As Integer, ByVal Y As Integer)
        '随机设置粒子初始角度和速度
        Dim Angle As Integer = Random.Next(AngleRange(0), AngleRange(1))
        Dim Velocity As Integer = Random.Next(VelocityRange(0), VelocityRange(1))

        '生成一个粒子演示并配置属性
        Dim ParticleDemo As Particle
        With ParticleDemo
            '发射位置
            .X = X
            .Y = Y
            'X、Y轴初始速度
            .XVelocity = Math.Sin((Math.PI / 180) * Angle) * Velocity * IIf(Random.Next > 0.5, 1, -1) 'IIf(,,)决定粒子左右方向
            .YVelocity = -Math.Cos((Math.PI / 180) * Angle) * Velocity
            '初始化透明度和直径
            .Alpha = DefaultAlpha
            .Diameter = DefaultDiameter
            '随机设置粒子颜色和生命长度
            .Color = Color.FromArgb(Random.Next(256), Random.Next(256), Random.Next(256))
            .LifeLength = Random.Next(LifeRange(0), LifeRange(1))
            .ResidualLife = .LifeLength
        End With
        '注册粒子
        Particles.Add(ParticleDemo)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '窗体启动，全屏显示，设置窗口图标
        Me.Location = New Point(0, 0)
        Me.Size = My.Computer.Screen.Bounds.Size
        Me.Icon = My.Resources.TestResource.ParticleEmitter
        '启用双缓冲绘图，拉伸背景
        Me.DoubleBuffered = True
        Me.BackgroundImageLayout = ImageLayout.Stretch
    End Sub

    Private Sub ParticleUpdate_Tick(sender As Object, e As EventArgs) Handles ParticleUpdate.Tick
        For TempIndex As Integer = 0 To 2
            EmitSingleParticle(MousePosition.X - DefaultDiameter / 2, MousePosition.Y)
        Next

        '间隔自动更新粒子状态和位图
        If Particles.Count = 0 Then Exit Sub
        '存在有存活的粒子
        ParticleBitmap = Nothing
        '刷新位图和画笔
        ParticleBitmap = New Bitmap(My.Resources.TestResource.壁纸, Me.Width, Me.Height)
        ParticleGraphics = Graphics.FromImage(ParticleBitmap)
        '变量表示循环因子和循环条件
        Dim Index As Integer = 0, ParticlesCount As Integer = Particles.Count
        Do Until Index = ParticlesCount
            '更新粒子状态（必须传入要处理粒子的真实内存地址）
            UpdateParticle(Particles.Item(Index), DefaultIntervalTime)
            If Particles.Item(Index).ResidualLife <= 0 Then
                '粒子死去，清空内存，解除注册，更新粒子总数
                Particles.Item(Index) = Nothing
                Particles.RemoveAt(Index)
                ParticlesCount -= 1
                '不绘制到位图，也不改变循环节，跳出一次循环
                Continue Do
            End If
            Dim ParticleInstance As Particle
            ParticleInstance = CType(Particles.Item(Index), Particle)

            '根据透明度和粒子颜色，定制画笔
            Dim ParticleBrush As New SolidBrush(Color.FromArgb(ParticleInstance.Alpha, ParticleInstance.Color))
            '根据定制画刷和粒子直径，绘制到位图，释放画刷内存
            ParticleGraphics.FillEllipse(ParticleBrush, ParticleInstance.X, ParticleInstance.Y, ParticleInstance.Diameter, ParticleInstance.Diameter)

            ParticleBrush.Dispose()
            '改变循环因子
            Index += 1
        Loop
        '在屏幕右上角显示现在存活的粒子总数
        ParticleGraphics.DrawString(ParticlesCount, Me.Font, Brushes.Red, 10, 10)
        '显示位图，释放画笔内存
        Me.BackgroundImage = ParticleBitmap
        ParticleGraphics.Dispose()
    End Sub

    Private Sub ParticleEmitter_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        EmitSingleParticle(MousePosition.X - DefaultDiameter / 2, MousePosition.Y)
    End Sub

End Class
