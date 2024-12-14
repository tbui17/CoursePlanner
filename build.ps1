param(
    [string]$cmd = "build",
    [string]$scriptcmd = "publish_upload"
)

function Build
{
    $build_args = @{
        KEY_URI = $env:KeyUri
    }

    $secrets = @{
        TENANT_ID = $env:TENANT_ID
        SERVICE_PRINCIPAL_ID = $env:SERVICE_PRINCIPAL_ID
        SERVICE_PRINCIPAL_SECRET = $env:SERVICE_PRINCIPAL_SECRET
    }

    $build_str = $build_args.GetEnumerator() | foreach { "--build-arg '$( $_.Key )=$( $_.Value )'" }
    $secret_str = $secrets.Keys | foreach { "--secret id=$_,env=$_" }
    $additional_args = $build_str + " " + $secret_str

    $file = "Scripts/Dockerfile"
    $currentDir = "."
    $imageTag = "build_image"

    $args = @(
        "docker build $currentDir"
        "--file $file"
        "--tag $imageTag"
        "--progress=plain"
        $additional_args
    )

    $expr = $args -join " "
    echo "Executing $expr"

    iex $expr
}

function Auth
{
    $keys = @( "TENANT_ID", "SERVICE_PRINCIPAL_ID", "SERVICE_PRINCIPAL_SECRET" )

    $vars = @{ }
    $keys | foreach {
        $secretValue = gc "/run/secrets/$_"
        $vars[$_] = $secretValue
    }

    az login --service-principal --username $vars.SERVICE_PRINCIPAL_ID --password $vars.SERVICE_PRINCIPAL_SECRET --tenant $vars.TENANT_ID
}

function StartApp
{
    dotnet run --verbosity normal --project Scripts -- $scriptcmd
}




function Main
{
    $path = $Script:MyInvocation.MyCommand.Path

    $cmds = Get-Command | where { $_.ScriptBlock.File -eq $path }

    $fn = $cmds | where Name -ilike $cmd | select -First 1

    if (!$fn)
    {
        Build
        return
    }
    iex $fn.Name
}

Main

