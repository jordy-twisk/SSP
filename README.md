# TinderClone
API for MBO &amp; HBO students 

<!---
### Submission details:

- API is hosted at 'https://dev-tinderfunv2.azurewebsites.net'
- API source located at 'TinderClone/src/cs/'
- API tests located at 'TinderClone/tests/'
- API specification located at 'TinderClone/src/resources/'


### Implementation code

- [] Implement `amountOfMessages` according to the YAML in `GetAllMessages()`.

- [] Calls for Coach, CoachTutorant and Tutorant. Mike (Coach and CoachTutorant) and Kaydo (Tutorant)

- [] Chat functionality. Mostly Kaydo and Barend

- [] API Tests. Mostly Kaydo and Thomas

- [] Exception handling security. Mostly Barend and Thomas.

- [] Database management. Barend

### YAML

- [] Verify YAML matches CoachService

- [✓] Verify YAML matches CoachTutorantService

- [✓] Verify YAML matches MessageService

- [✓] Verify YAML matches TutorantService

- [✓] Verify YAML matches UserServices

### Report

- [] Database subquestion. Mike

- [] Tests. Kaydo, Thomas

### Friday 1 November | Quick Notes
- [✓] SQL injection in UserService.cs
- [] Finalize the MessageService.cs
- [ONLY FOR StudentSERVICE] Generalize file structuur in every Controller and Service
- [✓] Structure and relations in the database
- [✓] Transport generic methods to external files
- [✓] Update yaml
- [✓] Delete unnecessary files
=======

### Verifying all files

A list of all files which are completed, tested and (if needed) match their API description:

| File                  | Checked by | Checked on (dd/mm/yyyy) | Latest Version |
| CoachTutorantServices | Mike       | 31/10/2019              | yes            |
| StudentServices       | Barend     | 01/11/2019              | yes            |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |
|                       |            | dd/mm/yyyy              | yes/no         |

-->

### LINKS
- https://www.coreycleary.me/project-structure-for-an-express-rest-api-when-there-is-no-standard-way/
- https://github.com/rapid7/metasploitable3 

### TODO 23:55 21 November 2019 :
	- FIND A WAY TO MAKE A GENERIC SOLUTION FOR ALL THE IF STATEMENTS IN COACHSERVICE
  - READ INPUT (REQUEST BODY && QUERY PARAMTERS)
  - SEND INPUT TO SERVICE
  - SERVICE HANDLES THE INPUT AND CALLS THE DATABASE LAYER
	- SERVICE GETS THE REQUESTED DATA AND RETURNS THE DATA TO THE CONTROLLER 
	- CONTROLLER RETURNS THE HTTPMESSAGE


    Test requestBody for POST:
            {
                "coach":{
	                "studentID": 123124,
	                "workload": 0
                },
                "student": {
	                "studentID": 123124,
	                "fistName": "Barend",
	                "lastName": "Testing"
                }
            }   
            
