Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Windows.Forms

Public Class Form1
    Inherits Form

    Private ReadOnly dtpRetirement As New DateTimePicker()
    Private ReadOnly cbEndOfDay As New CheckBox()
    Private ReadOnly lblYearsMonths As New Label()
    Private ReadOnly lblDays As New Label()
    Private ReadOnly lblHoursMinutes As New Label()
    Private ReadOnly tmrUpdate As New Timer()

    Public Sub New()
        Me.Text = "Retirement Countdown"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Width = 520
        Me.Height = 330
        Me.MinimizeBox = True
        Me.MaximizeBox = True

        ' Retirement date picker
        dtpRetirement.Format = DateTimePickerFormat.Custom
        dtpRetirement.CustomFormat = "MMMM dd, yyyy"
        dtpRetirement.Width = 220
        dtpRetirement.Value = DateTime.Now.Date.AddYears(1)   ' Default: 1 year ahead
        dtpRetirement.Location = New Drawing.Point(20, 20)

        ' End-of-day checkbox
        cbEndOfDay.Text = "Retire at end of day (11:59 PM)"
        cbEndOfDay.AutoSize = True
        cbEndOfDay.Location = New Drawing.Point(260, 23)
        cbEndOfDay.Checked = False

        ' Display 1: Years & Months
        Dim gb1 As New GroupBox() With {
            .Text = "Display 1: Years and Months until Retirement",
            .Location = New Drawing.Point(20, 60),
            .Width = 460,
            .Height = 70
        }
        lblYearsMonths.AutoSize = False
        lblYearsMonths.Location = New Drawing.Point(15, 30)
        lblYearsMonths.Width = gb1.Width - 30
        lblYearsMonths.Height = 24
        lblYearsMonths.Font = New Drawing.Font("Segoe UI", 11.0!, Drawing.FontStyle.Bold)
        gb1.Controls.Add(lblYearsMonths)

        ' Display 2: Days remaining
        Dim gb2 As New GroupBox() With {
            .Text = "Display 2: Days from Today until Retirement",
            .Location = New Drawing.Point(20, 140),
            .Width = 460,
            .Height = 70
        }
        lblDays.AutoSize = False
        lblDays.Location = New Drawing.Point(15, 30)
        lblDays.Width = gb2.Width - 30
        lblDays.Height = 24
        lblDays.Font = New Drawing.Font("Segoe UI", 11.0!, Drawing.FontStyle.Bold)
        gb2.Controls.Add(lblDays)

        ' Display 3: Hours & Minutes remaining
        Dim gb3 As New GroupBox() With {
            .Text = "Display 3: Hours and Minutes until Retirement",
            .Location = New Drawing.Point(20, 220),
            .Width = 460,
            .Height = 70
        }
        lblHoursMinutes.AutoSize = False
        lblHoursMinutes.Location = New Drawing.Point(15, 30)
        lblHoursMinutes.Width = gb3.Width - 30
        lblHoursMinutes.Height = 24
        lblHoursMinutes.Font = New Drawing.Font("Segoe UI", 11.0!, Drawing.FontStyle.Bold)
        gb3.Controls.Add(lblHoursMinutes)

        ' Add controls
        Me.Controls.Add(dtpRetirement)
        Me.Controls.Add(cbEndOfDay)
        Me.Controls.Add(gb1)
        Me.Controls.Add(gb2)
        Me.Controls.Add(gb3)

        ' Hook events
        AddHandler Me.Load, AddressOf Form_Load
        AddHandler dtpRetirement.ValueChanged, AddressOf OnInputsChanged
        AddHandler cbEndOfDay.CheckedChanged, AddressOf OnInputsChanged
        AddHandler tmrUpdate.Tick, AddressOf TmrUpdate_Tick
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs)
        ' Initial update and start the minute-synchronized timer
        UpdateDisplays()
        ScheduleNextTick()
        tmrUpdate.Start()
    End Sub

    Private Sub OnInputsChanged(sender As Object, e As EventArgs)
        UpdateDisplays()
        ' Re-sync the timer so the next tick lands on the next minute boundary
        ScheduleNextTick()
    End Sub

    Private Sub TmrUpdate_Tick(sender As Object, e As EventArgs)
        UpdateDisplays()
        ScheduleNextTick()
    End Sub

    ''' <summary>
    ''' Aligns the timer so updates land exactly on the next minute boundary (e.g., 12:34:00, 12:35:00...)
    ''' This prevents visible drift from a fixed 60,000 ms interval.
    ''' </summary>
    Private Sub ScheduleNextTick()
        Dim now As DateTime = DateTime.Now
        Dim nextMinute As DateTime = New DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1)
        Dim ms As Integer = CInt((nextMinute - now).TotalMilliseconds)
        If ms < 1 Then ms = 1000 ' Fallback
        tmrUpdate.Interval = ms
    End Sub

    Private Sub UpdateDisplays()
        Dim now As DateTime = DateTime.Now
        Dim targetDateOnly As DateTime = dtpRetirement.Value.Date

        ' Determine the exact target date-time:
        ' - Start-of-day (12:00 AM) if unchecked
        ' - End-of-day (11:59:59..., modeled as midnight of the next day) if checked
        Dim targetDateTime As DateTime =
            If(cbEndOfDay.Checked, targetDateOnly.AddDays(1), targetDateOnly)

        ' If retirement is today or in the past, show zeros with a friendly message
        If targetDateTime <= now Then
            lblYearsMonths.Text = "0 years, 0 months (Retirement reached!)"
            lblDays.Text = "0 days"
            lblHoursMinutes.Text = "0 hours, 0 minutes"
            Return
        End If

        ' -------- Display 1: Years and Months ----------
        ' Calendar-based whole years and months (ignores remaining days within the month)
        Dim years As Integer = targetDateOnly.Year - now.Year
        Dim months As Integer = targetDateOnly.Month - now.Month

        ' If we haven't reached the target day-of-month yet, subtract one month
        If targetDateOnly.Day < now.Day Then
            months -= 1
        End If
        ' Normalize negative months by borrowing from years
        If months < 0 Then
            years -= 1
            months += 12
        End If
        If years < 0 Then years = 0
        If months < 0 Then months = 0

        lblYearsMonths.Text = $"{years} year(s), {months} month(s)"

        ' -------- Display 2: Days ----------
        ' Whole days between today's date and the retirement date (date-only)
        Dim daysLeft As Integer = CInt((targetDateOnly - now.Date).TotalDays)
        If daysLeft < 0 Then daysLeft = 0
        lblDays.Text = $"{daysLeft} day(s)"

        ' -------- Display 3: Hours and Minutes ----------
        Dim remaining As TimeSpan = targetDateTime - now
        If remaining.TotalMinutes < 0 Then remaining = TimeSpan.Zero

        ' Whole hours + remaining minutes
        Dim hoursLeft As Integer = CInt(Math.Floor(remaining.TotalHours))
        Dim minutesLeft As Integer = remaining.Minutes

        lblHoursMinutes.Text = $"{hoursLeft} hour(s), {minutesLeft} minute(s)"
    End Sub
End Class