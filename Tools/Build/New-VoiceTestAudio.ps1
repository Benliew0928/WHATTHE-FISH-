param([Parameter(Mandatory)][string]$OutputPath)
# Run with Windows PowerShell for its built-in System.Speech assembly.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Speech
$speech=New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
 $format=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(16000,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
 $speech.SetOutputToWaveFile($OutputPath,$format)
 $speech.Speak(('This is a synthetic game voice test. The crew can hear each other across all four islands. ' * 8))
} finally { $speech.Dispose() }
if((Get-Item -LiteralPath $OutputPath).Length -lt 320000){throw 'Synthetic speech generation failed.'}
