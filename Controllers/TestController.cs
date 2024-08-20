using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Helpers;
using Interfaces;
using Models;
using Services;

namespace Controllers
{
    public class TestController
    {
        private readonly FileReaderService _fileReaderService;
        private readonly IHttpService _HttpService;
        private readonly FileWriterService _fileWriterService;
        private static string _step = "";
        private static int index = 0;
        private static string errorText = "";
        private static List<TestStepComponent> _testStepComponents = new List<TestStepComponent>();

        public TestController(FileReaderService fileReaderService, IHttpService HttpService, FileWriterService fileWriterService)
        {
            _fileReaderService = fileReaderService;
            _HttpService = HttpService;
            _fileWriterService = fileWriterService;
        }
        public void IntegrationTesting(List<TestFileRunnerComponent> files)
        {
            ApiRequest[] apiRequests = PreProcess(files, true);
            TestingApi(apiRequests, true);
        }
        public void UnitTesting(List<TestFileRunnerComponent> files)
        {
            ApiRequest[] apiRequests = PreProcess(files);
            TestingApi(apiRequests);
        }
        private ApiRequest[] PreProcess(List<TestFileRunnerComponent> files, bool isIntegration = false)
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files provided for unit test.");
                throw new ArgumentException("No files provided for unit test.");
            }
            ApiRequest[] apiRequests = new ApiRequest[files.Count];
            int i = 0;
            foreach (TestFileRunnerComponent file in files) // เตรียมข้อมูลสำหรับการ Test
            {
                ApiRequest apiRequest = _fileReaderService.ReadTestFileAsync(file.File, _fileWriterService.GetDictionaryPath()).Result;
                apiRequest = SelectTestCases(apiRequest, file.Ids, i + 1, file.File, isIntegration);
                apiRequests[i++] = apiRequest;
            }
            return apiRequests;
        }
        private void TestingApi(ApiRequest[] apiRequests, bool isCaptureMode = false)
        {
            ReportTesting reportTesting = new ReportTesting();
            reportTesting._DateTime = _fileWriterService.GetDateTime();
            reportTesting.Title = isCaptureMode ? "Integration Testing" : "Unit Testing";
            reportTesting.Status = "Running";
            int mainStep = 1;
            int totalTestCases = 0;
            int PassedTestCases = 0;
            int FailedTestCases = 0;
            int SkippedTestCases = 0;
            Dictionary<string, object> captureMain = new Dictionary<string, object>();
            bool isSkip = false;

            foreach (ApiRequest apiRequest in apiRequests)
            {
                Console.WriteLine($"\n\n######################################\n");
                int subStep = 1;
                if (apiRequest.Cases == null)
                {
                    Console.WriteLine("No test cases found.");
                    continue;
                }
                // pasting capture in to ApiRequest
                if (isCaptureMode)
                {

                    for (int i = 0; i < captureMain.Count; i++)
                    {
                        Converter.UpdateValue(apiRequest, captureMain.Keys.ElementAt(i), captureMain.Values.ElementAt(i));
                        ConsoleHelper.PrintInfo($"\n\t- Pasting Capture {captureMain.Keys.ElementAt(i)} : {captureMain.Values.ElementAt(i)}");
                    }
                }
                foreach (TestCaseComponent testCase in apiRequest.Cases)
                {
                    totalTestCases++;
                    Console.WriteLine($"\n--------------------------------------\n");
                    _testStepComponents[index].TestStatus = "Running";
                    _step = $"{mainStep}.{subStep++}";
                    Console.WriteLine($"Running Step :{_step}\n  \n\ttest case id: {testCase.Id} \n\tname: {testCase.Name}\n\tdescription: {testCase.Description}\n");
                    if (isSkip)
                    {
                        ConsoleHelper.PrintWarning("Skip this test case.");
                        _testStepComponents[index].TestStatus = "Skipped";
                        SkippedTestCases++;
                        index++;
                        continue;
                    }
                    var requestBody = MakeRequestBody(apiRequest.Type, apiRequest, testCase);
                    var requestHeaders = apiRequest.GetRequestHeaders(testCase);
                    string fileName = $"request{_step}.json";
                    _testStepComponents[index].RequestSource = "./" + Path.Combine("request", fileName).Replace('\\', '/');
                    HttpResponseMessage response = _HttpService.Request(apiRequest.Url, requestHeaders, apiRequest.Method, apiRequest.Type, requestBody, _step);
                    saveResponse(response);
                    Console.WriteLine($"\nResponse body: {response.Content.ReadAsStringAsync().Result}");
                    var (isAssert, inTestCaptures) = AssertingResponse(testCase, response, apiRequest);
                    if (!isAssert)
                    {
                        if (isCaptureMode)
                        {
                            errorText = $"\nIntegration test Failed at step {_step}";
                            ConsoleHelper.PrintError(errorText);
                            isSkip = true;
                        }
                        FailedTestCases++;
                        index++;
                        continue;
                    }
                    for (int i = 0; i < inTestCaptures!.Count; i++)
                    {
                        captureMain[inTestCaptures.Keys.ElementAt(i)] = inTestCaptures.Values.ElementAt(i);
                    }
                    PassedTestCases++;
                    index++;
                }
                mainStep++;
            }
            Console.WriteLine("\n\n--------------------------------------\n");
            string summary = $"\n\n\tTotal test cases: {totalTestCases}\n\tPassed test cases: {PassedTestCases}\n\tFailed test cases: {FailedTestCases}\n\tSkipped test cases: {SkippedTestCases}\n\n";
            ConsoleHelper.PrintResult(summary, FailedTestCases == 0);
            if (FailedTestCases != 0) ConsoleHelper.PrintError(errorText + "\n");
            reportTesting.Status = FailedTestCases == 0 ? "Passed" : "Failed";
            reportTesting.Duration = ((DateTime.Now - reportTesting._DateTime).Value.TotalSeconds).ToString("0.0000000") + " seconds";
            reportTesting.TestSteps = _testStepComponents;
            reportTesting.Error = errorText;
            _fileWriterService.WriteJson("report.json", reportTesting);
        }
        private void saveResponse(HttpResponseMessage response)
        {
            string fileExtension = response.Content.Headers.ContentType?.MediaType?.Contains("application/json") == true ? ".json" : ".txt";
            _fileWriterService.WriteJson($"response\\response{_step}.json", SerializeHttpResponseMessage(response));
            if (fileExtension == ".json")
            {
                Dictionary<string, object> jsonResponse = JsonHelper.ParseJson(response.Content.ReadAsStringAsync().Result);
                _fileWriterService.WriteJson($"response\\row_response{_step}.json", jsonResponse);
                _testStepComponents[index].ResponseBody = jsonResponse;
            }
            else
            {
                _fileWriterService.WriteText($"response\\row_response{_step}{fileExtension}", response.Content.ReadAsStringAsync().Result);
                _testStepComponents[index].ResponseBody = response.Content.ReadAsStringAsync().Result;
            }
            _testStepComponents[index].ResponseSource = "./" + Path.Combine("response", $"response{_step}{fileExtension}").Replace('\\', '/');
        }
        private static object SerializeHttpResponseMessage(HttpResponseMessage response)
        {
            // สร้างแหล่งข้อมูลสำหรับ HttpResponseMessage
            var responseData = new
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(
                    h => h.Key,
                    h => h.Value
                ),
                Content = response.Content.Headers.ContentType?.MediaType?.Contains("application/json") == true
                    ? JsonHelper.ParseJson(response.Content.ReadAsStringAsync().Result)
                    : new Dictionary<string, object> { { "xml", response.Content.ReadAsStringAsync().Result } }
            };
            return responseData;
        }
        private (bool, Dictionary<string, object>?) AssertingResponse(TestCaseComponent testCase, HttpResponseMessage httpResponseMessage, ApiRequest apiRequest)
        {
            bool isAssert;
            var capturesMain = new Dictionary<string, object>();
            var capturesHeaders = new List<CapturesComponent>();
            var capturesResponseBody = new List<CapturesComponent>();
            foreach (var (c, p) in (apiRequest.Captures ?? Enumerable.Empty<CapturesComponent>()).Select(x => (x.CapturePath, x.PassPath)))
            {
                var (captureMode, capture) = Converter.ExtractModeAndKey(c);
                if (captureMode == 1)
                {
                    capturesHeaders.Add(new CapturesComponent { CapturePath = capture, PassPath = p });
                }
                else
                {
                    capturesResponseBody.Add(new CapturesComponent { CapturePath = capture, PassPath = p });
                }
            }
            // Assert Status Code
            isAssert = AssertStatusCode(testCase, httpResponseMessage);
            if (!isAssert) return (false, null);

            // Assert Response Headers
            isAssert = AssertHeaders(testCase, httpResponseMessage, capturesHeaders, capturesMain);
            if (!isAssert) return (false, null);

            // Assert Response Body
            isAssert = AssertResponseBody(testCase, httpResponseMessage, capturesResponseBody, capturesMain);
            // log capture
            for (int i = 0; i < capturesMain.Count; i++)
            {
                ConsoleHelper.PrintInfo($"\n\t- Capture {capturesMain.Keys.ElementAt(i)} : {capturesMain.Values.ElementAt(i)}");
            }
            return (isAssert, capturesMain);
        }

        private bool AssertStatusCode(TestCaseComponent testCase, HttpResponseMessage httpResponseMessage)
        {
            _testStepComponents[index].ResponseStatusCode = (int)httpResponseMessage.StatusCode;
            if (testCase.Status == 0) return true;

            Console.WriteLine("\n Assert Response status code :\n\n");
            Console.WriteLine($"\t Expected status code: {testCase.Status}");
            Console.WriteLine("\t Actual status code: " + (int)httpResponseMessage.StatusCode);

            bool isStatusAssert = testCase.Status == (int)httpResponseMessage.StatusCode;
            SubAssertComponent statusAssert = new SubAssertComponent
            {
                Key = "Status Code",
                Expected = testCase.Status,
                Actual = (int)httpResponseMessage.StatusCode,
                Operator = "(==)",
                IsAssert = isStatusAssert
            };
            _testStepComponents[index].Assert!.ResponseStatusCode = new List<SubAssertComponent> { statusAssert };
            ConsoleHelper.PrintResult("\t Assert Status: " + (isStatusAssert ? "Passed" : "Failed"), isStatusAssert);

            if (!isStatusAssert)
            {
                errorText = $"\nTest case {testCase.Id} failed. Expected status code: {testCase.Status}, but got: {httpResponseMessage.StatusCode}";
                _testStepComponents[index].Error = errorText;
                ConsoleHelper.PrintError($"\nTest case {testCase.Id} failed. Expected status code: {testCase.Status}, but got: {httpResponseMessage.StatusCode}");

                return false;
            }

            return true;
        }

        private bool AssertHeaders(TestCaseComponent testCase, HttpResponseMessage httpResponseMessage, List<CapturesComponent> capturesHeaders, Dictionary<string, object> capturesMain)
        {
            _testStepComponents[index].ResponseHeader = httpResponseMessage.Headers;
            if (testCase.AssertHeader == null) return true;
            var headersAssert = new List<SubAssertComponent>();
            Console.WriteLine($"\n Assert Header:");
            foreach (var (k, expectedValue) in testCase.AssertHeader)
            {
                var headerAssert = new SubAssertComponent();
                var (key, modesStr) = AssertHelper.AssertActualTool(k);
                bool isCount = modesStr.Contains("(count)");
                if (isCount) modesStr = modesStr.Replace("(count)", "").Replace("|", "");
                Console.WriteLine($"\n\t key: {key} {(isCount ? "(count)" : "")}");
                headerAssert.Key = $"{key}{(isCount ? " (count)" : "")}";
                headerAssert.Expected = expectedValue;
                headerAssert.Operator = modesStr;
                if (!httpResponseMessage.Headers.TryGetValues(key, out IEnumerable<string>? values))
                {
                    headerAssert.Actual = null;
                    ConsoleHelper.PrintError($"\t Key: {key} not found in response header");
                    errorText = $"\nTest case {testCase.Id} failed. Key: {key} not found in response header";
                    _testStepComponents[index].Error = errorText;
                    Console.WriteLine("\n\t header: ");
                    foreach (var (header, v) in httpResponseMessage.Headers)
                    {
                        Console.WriteLine($"\t {header}:");
                        foreach (var value in v)
                        {
                            Console.WriteLine($"\t\t {value}");
                        }
                    }
                    return false;
                }

                object? actualValue = GetActualHeaderValue(values, isCount);
                headerAssert.Actual = actualValue;
                Console.WriteLine("\t Actual: " + actualValue);
                if (!AssertHelper.Assert(actualValue, expectedValue, modesStr))
                {
                    errorText = $"\nTest case {testCase.Id} failed. Key{headerAssert.Key}, Expected: {expectedValue}, but got: {actualValue}";
                    ConsoleHelper.PrintError($"\t Assert Result: Failed");
                    _testStepComponents[index].Error = errorText;
                    return false;
                }
                headerAssert.IsAssert = true;
                ConsoleHelper.PrintSuccess($"\t Assert Result: Passed");
                headersAssert.Add(headerAssert);
            }
            _testStepComponents[index]!.Assert!.ResponseHeader = headersAssert;
            // Capture Headers
            foreach (var c in capturesHeaders)
            {
                var (key, modesStr) = AssertHelper.AssertActualTool(c.CapturePath);
                bool isCount = modesStr.Contains("(count)");
                if (!httpResponseMessage.Headers.TryGetValues(key, out IEnumerable<string>? values)) continue;
                object? actualValue = GetActualHeaderValue(values, isCount);
                capturesMain[c.PassPath] = actualValue;
            }
            return true;
        }

        private object GetActualHeaderValue(IEnumerable<string> values, bool isCount)
        {
            if (isCount) return values.Count();

            if (values.Count() > 1) return string.Join(",", values);

            else return values.FirstOrDefault() ?? string.Empty;
        }

        private bool AssertResponseBody(TestCaseComponent testCase, HttpResponseMessage httpResponseMessage, List<CapturesComponent> capturesResponseBody, Dictionary<string, object> capturesMain)
        {
            _testStepComponents[index].ResponseType = httpResponseMessage.Content.Headers.ContentType?.MediaType?.Replace("application/", "") ?? string.Empty;
            if (testCase.AssertResponse == null) return true;
            var assertsResponseBody = new List<SubAssertComponent>();
            Console.WriteLine($"\nAssert Response: \n\n");

            string responseBodyStr = httpResponseMessage.Content.ReadAsStringAsync().Result;
            string contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (contentType.Contains("application/xml"))
            {
                responseBodyStr = XmlHelper.XmlToJson(responseBodyStr);
            }

            var responseBody = JsonHelper.ParseJson(responseBodyStr);
            bool hasError = false;
            foreach (var (k, expectedValue) in testCase.AssertResponse)
            {
                var assertResponseBody = new SubAssertComponent();
                var (key, modesStr) = AssertHelper.AssertActualTool(k);
                var actualValue = JsonHelper.GetValue(responseBody, key);
                bool isCount = modesStr.Contains("(count)");
                if (isCount) modesStr = modesStr.Replace("(count)", "").Replace("|", "");
                Console.WriteLine($"\n\t key: {key} {(isCount ? "(count)" : "")}");
                assertResponseBody.Key = $"{key}{(isCount ? " (count)" : "")}";
                assertResponseBody.Expected = expectedValue;
                assertResponseBody.Operator = modesStr;
                assertResponseBody.Actual = actualValue;
                if (isCount)
                {
                    if (actualValue is IEnumerable<object> enumerable)
                    {
                        actualValue = enumerable.Count();
                    }
                    else
                    {
                        errorText = $"\nTest case {testCase.Id} failed. Key {assertResponseBody.Key}, Actual value is not enumerable";
                        ConsoleHelper.PrintError($"\t Actual value is not enumerable");
                        _testStepComponents[index].Error = errorText;
                        hasError = true;
                        assertsResponseBody.Add(assertResponseBody);
                        continue;
                    }
                }

                if (!AssertHelper.Assert(actualValue!, expectedValue, modesStr))
                {
                    errorText = $"\nTest case {testCase.Id} failed. Key {assertResponseBody.Key}, Expected: {expectedValue}, but got: {actualValue}";
                    ConsoleHelper.PrintError($"\t Assert Result: Failed");
                    _testStepComponents[index].Error = errorText;
                    hasError = true;
                    assertsResponseBody.Add(assertResponseBody);
                    continue;
                }

                ConsoleHelper.PrintSuccess($"\t Assert Result: Passed");
                assertResponseBody.IsAssert = true;
                assertsResponseBody.Add(assertResponseBody);
            }
            _testStepComponents[index].Assert!.ResponseBody = assertsResponseBody;

            // Capture Response Body
            foreach (var c in capturesResponseBody)
            {
                var (key, modesStr) = AssertHelper.AssertActualTool(c.CapturePath);
                var actualValue = JsonHelper.GetValue(responseBody, key);
                bool isCount = modesStr.Contains("(count)");
                if (isCount)
                {
                    if (actualValue is IEnumerable<object> enumerable)
                    {
                        actualValue = enumerable.Count();
                    }
                    else
                    {
                        ConsoleHelper.PrintError($"\t Actual value is not enumerable");
                        hasError = true;
                        continue;
                    }
                }
                capturesMain[c.PassPath] = actualValue ?? string.Empty;
            }
            return !hasError;
        }

        private object MakeRequestBody(string? type, ApiRequest apiRequest, TestCaseComponent testCase)
        {
            if (type == "json")
            {
                return JsonSerializer.Serialize(apiRequest.GetRequestBodyJson(testCase));
            }
            else if (type == "form")
            {
                Dictionary<string, object> requestBody = apiRequest.GetRequestBodyJson(testCase);
                var formData = requestBody.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString() ?? ""));
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(formData);
                _testStepComponents[index].RequestBody = formContent.ReadAsStringAsync().Result;
                return formContent;
            }
            else if (type == "xml")
            {
                _testStepComponents[index].RequestBody = apiRequest.GetRequestBodyXml(testCase);
                return apiRequest.GetRequestBodyXml(testCase);
            }
            else
            {
                return "";
            }
        }
        private ApiRequest SelectTestCases(ApiRequest apiRequest, List<object>? ids, int mainStep, string filePath, bool isIntegration = false)
        {
            // ตรวจสอบค่า null และทำให้แน่ใจว่า ids มีข้อมูล
            if (ids == null || ids.Count == 0 || ids[0]?.ToString() == "*")
            {
                apiRequest.Cases = apiRequest.Cases ?? new List<TestCaseComponent>();
            }
            // ตรวจสอบว่า apiRequest.Cases มีข้อมูลหรือไม่
            else if (apiRequest.Cases == null)
            {
                apiRequest.Cases = new List<TestCaseComponent>();
            }
            else
            {
                var selectedCaseIds = new List<int>();

                foreach (object id in ids)
                {
                    string? idString = id?.ToString();

                    if (string.IsNullOrEmpty(idString)) continue;

                    if (idString.Contains("-")) // กรณีที่เป็น range ของ id
                    {
                        string[] range = idString.Split('-');

                        if (range.Length != 2 ||
                            !int.TryParse(range[0], out int start) ||
                            !int.TryParse(range[1], out int end)) // ตรวจสอบว่าเป็น range ที่ถูกต้องหรือไม่
                        {
                            throw new ArgumentException($"Invalid range id: {idString}");
                        }

                        if (start > end) // ถ้า start มากกว่า end ให้เรียงย้อนหลัง
                        {
                            for (int i = start; i >= end; i--)
                            {
                                selectedCaseIds.Add(i);
                            }
                        }
                        else
                        {
                            for (int i = start; i <= end; i++)
                            {
                                selectedCaseIds.Add(i);
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(idString, out int caseId))
                        {
                            selectedCaseIds.Add(caseId);
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid id format: {idString}");
                        }
                    }
                }

                // ฟิลเตอร์ `apiRequest.Cases` ด้วย `selectedCaseIds`
                apiRequest.Cases = apiRequest.Cases
                    .Where(c => selectedCaseIds.Contains(c.Id))
                    .ToList();

                // จัดเรียง `apiRequest.Cases` ตามลำดับที่ `selectedCaseIds` ระบุ
                apiRequest.Cases = apiRequest.Cases
                    .OrderBy(c => selectedCaseIds.IndexOf(c.Id))
                    .ToList();
            }

            // ใส่ข้อมูลลงใน `TestStepComponent`
            apiRequest.SetType();
            for (int i = 0; i < apiRequest.Cases.Count; i++)
            {
                TestCaseComponent testCase = apiRequest.Cases[i];
                TestStepComponent testStepComponent = new TestStepComponent
                {
                    Step = $"{mainStep}.{i + 1}",
                    File = "./" + Path.Combine("source", Path.GetFileName(filePath)).Replace("\\", "/"),
                    CaseID = testCase.Id,
                    CaseName = testCase.Name,
                    CaseDescription = testCase.Description,
                    CaseTags = testCase.Tags,
                    RequestSource = "./" + Path.Combine("request", $"request{mainStep}.{i + 1}.json"),
                    RequestURL = apiRequest.Url,
                    RequestMethod = apiRequest.Method,
                    RequestType = apiRequest.Type,
                    RequestHeader = Converter.ConvertingDictionaryObjectToListString(apiRequest.GetRequestHeaders(testCase)),
                    RequestBody = apiRequest.GetRequestBodyJson(testCase),
                    ResponseBody = apiRequest.Type == "json" ? apiRequest.GetRequestBodyJson(testCase) : apiRequest.GetRequestBodyXml(testCase),
                    TestStatus = "Waiting",
                    TestType = isIntegration ? "Integration" : "Unit",
                    Assert = new AssertComponent()
                };
                _testStepComponents.Add(testStepComponent);
            }

            return apiRequest;
        }


    }
}