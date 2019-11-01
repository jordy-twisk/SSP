def api_link():
  return "https://dev-tinderfunv2-test.azurewebsites.net/api/"
def studentId():
  return "581433"
def coachId():
  return "701"
def tutorantId():
  return "702"
def messageId():
  return (coachId() + tutorantId())

def create_coach():
  import requests
  url = api_link() + "profile/coach"
  payload = "{\n    \"coach\": {\n      \"studentID\": \""+ coachId() + "\",\n      \"workload\": 10\n    },\n  \n    \"user\": {\n      \"studentID\": \""+ coachId() + "\",\n      \"firstName\": \"TestCoach\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a coach\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_coach():
  import requests
  r = requests.delete(api_link() + "profile/coach/" + coachId())
def create_tutorant():
  import requests
  url = api_link() + "profile/tutorant"
  payload = "{\n   \"user\": {\n      \"studentID\": \""+ tutorantId() + "\",\n      \"firstName\": \"TestTutorant\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a student\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }, \n  \"tutorant\": {\n      \"studentID\": \""+ tutorantId() + "\"\n } \n}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_tutorant():
  import requests
  r = requests.delete(api_link() + "profile/tutorant/" + tutorantId())  

def s_studentData():
  studentData = {	
    'studentID': 	  {'type': 'integer'},
    'firstName': 	  {'type': 'string'},
    'surName':	    {'type': 'string'},
    'phoneNumber':	{'type': 'string'},
    'photo':	      {'type': 'string'},
    'description':	{'type': 'string'},
    'degree':	      {'type': 'string'},
    'study':	      {'type': 'string'},
    'studyYear':	  {'type': 'integer'},
    'interests':	  {'type': 'string'}
  }
  return studentData
def s_coach():
  coach = {
	  'studentID': 	{'type': 'integer'},
	  'workload':	{'type': 'integer'}
  }
  return coach