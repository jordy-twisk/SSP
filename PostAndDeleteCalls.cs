 /* 
 This doesnt belong in UserController anymore, use this code for the Coach en Tutorant Controllers

else if (req.Method == HttpMethod.Post) {
    string body = await req.Content.ReadAsStringAsync();

    using (StringReader reader = new StringReader(body)) {
        string json = reader.ReadToEnd();
        newStudent = JsonConvert.DeserializeObject<User>(json);
    }

    queryString = $"INSERT INTO [dbo].[Student] " +
        $"(studentID, firstName, surName, phoneNumber, photo, description, degree, study, studyYear, interests) " +
        $"VALUES " +
        $"({studentID}, '{newStudent.firstName}', '{newStudent.surName}', '{newStudent.phoneNumber}', '{newStudent.photo}', " +
        $"'{newStudent.description}', '{newStudent.degree}', '{newStudent.study}', {newStudent.studyYear}, '{newStudent.interests}');";

    log.LogInformation($"Executing the following query: {queryString}");

    SqlCommand command = new SqlCommand(queryString, connection);
    await command.ExecuteNonQueryAsync();

    httpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $" {studentID} Added to the database");
} 
else if (req.Method == HttpMethod.Delete){
    queryString = $"DELETE FROM [dbo].[Student] WHERE {studentID} = studentID;";

    log.LogInformation($"Executing the following query: {queryString}");

    SqlCommand command = new SqlCommand(queryString, connection);
    await command.ExecuteNonQueryAsync();

    httpResponseMessage = req.CreateResponse(HttpStatusCode.OK, $"{studentID} Deleted from the database");
} 

*/