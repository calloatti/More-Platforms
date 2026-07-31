# ================================================================================
# TIMBERMESH BATCH DECODER SYSTEM (BULLETPROOF VERSION)
# Automatically decodes all .timbermesh files in the current directory.
# Saves outputs as [FileName].timbermesh.txt
# ================================================================================

# 1. Scan Local Workspace Directory
$files = Get-ChildItem -Path . -Filter *.timbermesh

if ($files.Count -eq 0) {
    Write-Warning "No .timbermesh targets found in the current directory execution frame."
    return
}

Write-Host "Found $($files.Count) target files. Initializing batch execution loop..." -ForegroundColor Yellow

foreach ($file in $files) {
    $literalPath = $file.FullName
    $savePath = Join-Path $file.DirectoryName "$($file.Name).txt"

    Write-Host "----------------------------------------------------------------" -ForegroundColor Gray
    Write-Host "Reading binary asset stream: $($file.Name)..." -ForegroundColor Cyan
    $bytes = [System.IO.File]::ReadAllBytes($literalPath)

    if ($bytes.Length -lt 2) {
        Write-Error "Error: Binary file payload for $($file.Name) is missing or corrupted."
        continue
    }

    # 2. Extract Zlib Layer Framework
    if ($bytes[0] -eq 0x78) {
        $skipBytes = 2
        Write-Host "Zlib signature detected (0x789C). Allocating inflation stream..." -ForegroundColor Green
    } else {
        $skipBytes = 0
        Write-Warning "Missing standard zlib initialization vector for $($file.Name). Attempting raw parsing..."
    }

    try {
        $msInput = [System.IO.MemoryStream]::new($bytes, $skipBytes, $bytes.Length - $skipBytes)
        $deflate = [System.IO.Compression.DeflateStream]::new($msInput, [System.IO.Compression.CompressionMode]::Decompress)
        $msOutput = [System.IO.MemoryStream]::new()
        $deflate.CopyTo($msOutput)
        $protoBytes = $msOutput.ToArray()
        Write-Host "Inflation processing complete. Uncompressed payload size: $($protoBytes.Length) bytes." -ForegroundColor Green
    } catch {
        Write-Warning "Failed to inflate compressed block layer for $($file.Name). File may be corrupted or raw text."
        continue
    }

    # 3. Text Accumulation Engine Initialization
    $textLog = [System.Collections.Generic.List[string]]::new()
    $textLog.Add("================================================================================")
    $textLog.Add("TIMBERMESH STRUCTURAL DECODE")
    $textLog.Add("Source Asset: $($literalPath)")
    $textLog.Add("Export Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $textLog.Add("Uncompressed Wire Size: $($protoBytes.Length) bytes")
    $textLog.Add("================================================================================")

    # 4. Low-Level Protobuf Decoding Subsystems (Scoped per-file instance)
    function Read-Varint($stream) {
        $value = [uint64]0
        $shift = 0
        while ($true) {
            $b = $stream.ReadByte()
            if ($b -lt 0) { return $null }
            
            $part = [uint64]($b -band 0x7F)
            $value = $value -bor ($part -shl $shift)
            
            if (($b -band 0x80) -eq 0) { break }
            $shift += 7
            if ($shift -ge 64) { return $null }
        }
        return $value
    }

    function Parse-ProtobufStructure($payloadBytes, $messageType, $indent = "") {
        $stream = [System.IO.MemoryStream]::new($payloadBytes)
        $vpScalarType = 0
        $vpDimension = 1
        
        while ($stream.Position -lt $stream.Length) {
            try {
                $tag = Read-Varint $stream
                if ($null -eq $tag) { break }
                
                $wireType = $tag -band 0x7
                $fieldNumber = $tag -shr 3
                
                $varintVal = $null
                $fixed64Buf = $null
                $lengthDelimitedBuf = $null
                $fixed32Buf = $null
                
                switch ($wireType) {
                    0 { 
                        $varintVal = Read-Varint $stream 
                        if ($null -eq $varintVal) { throw "Truncated varint stream payload." }
                    }
                    1 {
                        $buf = New-Object byte[] 8
                        [void]$stream.Read($buf, 0, 8)
                        $fixed64Buf = $buf
                    }
                    2 {
                        $len = Read-Varint $stream
                        if ($null -eq $len) { throw "Truncated length prefix descriptor." }
                        
                        # CRITICAL SAFETY CHECK: Prevent massive out-of-bounds array allocations from corrupted bytes
                        if ($len -gt ($stream.Length - $stream.Position) -or $len -lt 0) {
                            throw "Malformed field size descriptor ($len) exceeds remaining stream allocation buffer."
                        }
                        
                        $buf = New-Object byte[] $len
                        if ($len -gt 0) { [void]$stream.Read($buf, 0, $len) }
                        $lengthDelimitedBuf = $buf
                    }
                    5 {
                        $buf = New-Object byte[] 4
                        [void]$stream.Read($buf, 0, 4)
                        $fixed32Buf = $buf
                    }
                    default {
                        $textLog.Add("${indent}!! Warning: Encountered unexpected wire allocation index ($wireType).")
                        return
                    }
                }
                
                switch ($messageType) {
                    "Model" {
                        switch ($fieldNumber) {
                            1 { $textLog.Add("${indent}Model Version = $varintVal") }
                            2 { $textLog.Add("${indent}Model Name = `"$([System.Text.Encoding]::ASCII.GetString($lengthDelimitedBuf))`"") }
                            3 {
                                $textLog.Add("${indent}Node Reference [repeated Node]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "Node" ($indent + "  ")
                            }
                        }
                    }
                    "Node" {
                        switch ($fieldNumber) {
                            1 {
                                $signedParent = $varintVal
                                if ($varintVal -eq 18446744073709551615) { $signedParent = -1 }
                                $textLog.Add("${indent}Parent Node ID = $signedParent")
                            }
                            2 { $textLog.Add("${indent}Node Name = `"$([System.Text.Encoding]::ASCII.GetString($lengthDelimitedBuf))`"") }
                            3 {
                                $textLog.Add("${indent}Transform Position [Vector3Float]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "Vector3Float" ($indent + "  ")
                            }
                            4 {
                                $textLog.Add("${indent}Transform Rotation [QuaternionFloat]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "QuaternionFloat" ($indent + "  ")
                            }
                            5 {
                                $textLog.Add("${indent}Transform Scale [Vector3Float]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "Vector3Float" ($indent + "  ")
                            }
                            6 { $textLog.Add("${indent}Total Vertex Count = $varintVal") }
                            7 {
                                $textLog.Add("${indent}Vertex Property [repeated VertexProperty]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "VertexProperty" ($indent + "  ")
                            }
                            8 {
                                $textLog.Add("${indent}Mesh Geometry Node [repeated Mesh]:")
                                Parse-ProtobufStructure $lengthDelimitedBuf "Mesh" ($indent + "  ")
                            }
                        }
                    }
                    "Vector3Float" {
                        $fVal = [System.BitConverter]::ToSingle($fixed32Buf, 0)
                        switch ($fieldNumber) {
                            1 { $textLog.Add("${indent}X = $fVal") }
                            2 { $textLog.Add("${indent}Y = $fVal") }
                            3 { $textLog.Add("${indent}Z = $fVal") }
                        }
                    }
                    "QuaternionFloat" {
                        $fVal = [System.BitConverter]::ToSingle($fixed32Buf, 0)
                        switch ($fieldNumber) {
                            1 { $textLog.Add("${indent}X = $fVal") }
                            2 { $textLog.Add("${indent}Y = $fVal") }
                            3 { $textLog.Add("${indent}Z = $fVal") }
                            4 { $textLog.Add("${indent}W = $fVal") }
                        }
                    }
                    "Mesh" {
                        switch ($fieldNumber) {
                            1 {
                                $idxStream = [System.IO.MemoryStream]::new($lengthDelimitedBuf)
                                $indices = [System.Collections.Generic.List[int]]::new()
                                while ($idxStream.Position -lt $idxStream.Length) {
                                    $idxVal = Read-Varint $idxStream
                                    if ($null -ne $idxVal) { $indices.Add([int]$idxVal) }
                                }
                                $textLog.Add("${indent}Indices Vector (Count=$($indices.Count)) = [" + ($indices -join ", ") + "]")
                            }
                            2 { $textLog.Add("${indent}Material Name = `"$([System.Text.Encoding]::ASCII.GetString($lengthDelimitedBuf))`"") }
                        }
                    }
                    "VertexProperty" {
                        switch ($fieldNumber) {
                            1 { $textLog.Add("${indent}Attribute ID = `"$([System.Text.Encoding]::ASCII.GetString($lengthDelimitedBuf))`"") }
                            2 {
                                $vpScalarType = [int]$varintVal
                                $textLog.Add("${indent}Scalar Encoding Type = $vpScalarType")
                            }
                            3 {
                                $vpDimension = [int]$varintVal
                                $textLog.Add("${indent}Vector Layout Dimension = $vpDimension")
                            }
                            4 {
                                if ($vpScalarType -eq 4 -and $vpDimension -gt 0) {
                                    $elemCount = $lengthDelimitedBuf.Length / 4
                                    $tuples = [System.Collections.Generic.List[string]]::new()
                                    for ($i = 0; $i -lt $elemCount; $i += $vpDimension) {
                                        $coords = @()
                                        for ($d = 0; $d -lt $vpDimension; $d++) {
                                            if ((($i + $d) * 4) -lt $lengthDelimitedBuf.Length) {
                                                $coords += [System.BitConverter]::ToSingle($lengthDelimitedBuf, (($i + $d) * 4))
                                            }
                                        }
                                        $tuples.Add("(" + ($coords -join ", ") + ")")
                                    }
                                    $textLog.Add("${indent}Decoded Scalar Data Matrix (Total Floats=$elemCount):")
                                    for ($m = 0; $m -lt $tuples.Count; $m += 4) {
                                        $endIdx = [Math]::Min($m+3, $tuples.Count-1)
                                        $chunk = $tuples[$m..$endIdx] -join "   "
                                        $textLog.Add("${indent}  [$m..$endIdx]: $chunk")
                                    }
                                } else {
                                    $hexSnippet = [System.BitConverter]::ToString($lengthDelimitedBuf)
                                    if ($hexSnippet.Length -gt 45) { $hexSnippet = $hexSnippet.Substring(0, 42) + "..." }
                                    $textLog.Add("${indent}Decoded Byte Segment = $hexSnippet")
                                }
                            }
                        }
                    }
                }
            } catch {
                # Catch any corruption bubble, dump it to log, and immediately break processing for this file
                $textLog.Add("${indent}!! Critical Parsing Fault: Stream block corrupted at byte context position $($stream.Position). Exception: $_")
                $stream.Position = $stream.Length # Instantly break the while loop cleanly
                break
            }
        }
    }

    # 5. Core Engine Execution Loop
    Write-Host "De-serializing mesh wireframes for $($file.Name)..." -ForegroundColor Yellow
    Parse-ProtobufStructure $protoBytes "Model"

    # 6. Disk IO Serialization Sequence
    try {
        [System.IO.File]::WriteAllLines($savePath, $textLog)
        Write-Host "Compiled dataset successfully -> $(Split-Path $savePath -Leaf)" -ForegroundColor Green
    } catch {
        Write-Error "Failed to write structured log data to disk for $($file.Name): $_"
    }
}

Write-Host "================================================================" -ForegroundColor Gray
Write-Host "Batch process execution complete." -ForegroundColor Green