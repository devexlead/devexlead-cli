namespace DevExLead.Modules.Export.Model
{
    internal class KubernetesSettingsFile
    {
        public Default Default { get; set; }
        public PR PR { get; set; }
        public Test Test { get; set; }
        public Stage Stage { get; set; }
        public Prod Prod { get; set; }
    }

    public class Default
    {
        public string app_name { get; set; }
        public string app_owner { get; set; }
        public string alert_group { get; set; }
        public int app_port { get; set; }
        public string base_template { get; set; }
        public string cpu_limit { get; set; }
        public string cpu_request { get; set; }
        public string docker_repo { get; set; }
        public string healthcheck_address { get; set; }
        public string healthcheck_protocol { get; set; }
        public string memory_limit { get; set; }
        public string memory_request { get; set; }
        public int min_replicas { get; set; }
        public int max_replicas { get; set; }
        public string sanity_command { get; set; }
        public string service_prefix { get; set; }
        public string service_visiblity { get; set; }
        public Env_Variables[] env_variables { get; set; }
    }

    public class Env_Variables
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class PR
    {
        public Env_Variables1[] env_variables { get; set; }
    }

    public class Env_Variables1
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Test
    {
        public Env_Variables2[] env_variables { get; set; }
    }

    public class Env_Variables2
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Stage
    {
        public Env_Variables3[] env_variables { get; set; }
        public int min_replicas { get; set; }
    }

    public class Env_Variables3
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Prod
    {
        public Env_Variables4[] env_variables { get; set; }
        public int min_replicas { get; set; }
        public int max_replicas { get; set; }
    }

    public class Env_Variables4
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}