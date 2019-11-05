#static values
def api_link():
  return "https://dev-tinderfunv2.azurewebsites.net/api/"
def studentId():
  return "581433"
def coachId():
  return "701"
def tutorantId():
  return "710"
def tutorantId1():
  return "711"
def tutorantId2():
  return "712"
def messageId():
  return (coachId() + tutorantId())

#creation or deletion for multiple things, one call.
def create_multiple_tutorant():
  create_tutorant(tutorantId())
  create_tutorant(tutorantId1())
  create_tutorant(tutorantId2())
def delete_multiple_tutorant():
  delete_tutorant(tutorantId())
  delete_tutorant(tutorantId1())
  delete_tutorant(tutorantId2())  
def create_multiple_coachTutorant():
  create_coachTutorant(tutorantId())
  create_coachTutorant(tutorantId1())
  create_coachTutorant(tutorantId2())
def delete_multiple_coachTutorant():
  delete_coachTutorant(coachId())

#creation or deletion of testing profiles.
import requests
def create_coach():
  url = api_link() + "profile/coach"
  payload = "{\n    \"coach\": {\n      \"studentID\": \""+ coachId() + "\",\n      \"workload\": 10\n    },\n  \n    \"user\": {\n      \"studentID\": \""+ coachId() + "\",\n      \"firstName\": \"TestCoach\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a coach\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_coach():
  r = requests.delete(api_link() + "profile/coach/" + coachId())
def create_tutorant(tut):
  url = api_link() + "profile/tutorant"
  payload = "{\n   \"user\": {\n      \"studentID\": \""+ tut + "\",\n      \"firstName\": \"TestTutorant\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a student\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n    }, \n  \"tutorant\": {\n      \"studentID\": \""+ tut + "\"\n } \n}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_tutorant(tut):
  r = requests.delete(api_link() + "profile/tutorant/" + tut)
def create_coachTutorant(tut):
  url = api_link() + "coachTutorant/tutorant/" + tut
  payload = "{\n\"studentIDTutorant\":\""+ tut +"\",\n\"studentIDCoach\": \""+ coachId() +"\",\n\"status\":\"Pending\"\n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  response = requests.request("POST", url, data=payload, headers=headers)
def delete_coachTutorant(tut):
  r = requests.delete(api_link() + "coachTutorant/coach/" + coachId())

#schema's for validation
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
def s_tutorant():
  tutorant = {
    'studentID': {'type': 'integer'}
  }
  return tutorant
def s_coachTutorant():
  coachTutorant = {
    'studentIDTutorant': {'type': 'integer'},
    'studentIDCoach': {'type': 'integer'},
    'status': {'type': 'string'}
  }
  return coachTutorant
