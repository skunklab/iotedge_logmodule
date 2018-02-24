
foreach($arg in $args)
{
   Import-Certificate -Filepath $arg -CertStoreLocation cert:\LocalMachine\TrustedPublisher
}

