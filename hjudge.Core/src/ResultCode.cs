namespace hjudge.Core
{
    public enum ResultCode : int
    {
        Pending = -1,
        Judging = 0,
        Accepted = 1,
        Wrong_Answer = 2,
        Compile_Error = 3,
        Time_Limit_Exceeded = 4,
        Memory_Limit_Exceeded = 5,
        Presentation_Error = 6,
        Runtime_Error = 7,
        Special_Judge_Error = 8,
        Problem_Config_Error = 9,
        Output_File_Error = 10,
        Unknown_Error = 11
    }
}