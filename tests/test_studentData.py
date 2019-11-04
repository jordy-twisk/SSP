import pytest
import requests
import os
import api_info as a
from cerberus import Validator

def test_get_student_byId():
  r = requests.get(a.api_link() + "student/" + a.studentId())
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_studentData())
  assert v.validate(r.json()) == True, v.errors
def test_put_student_byId():
  #fout ligt nog in de code. Wordt naar gekeken
  tut = a.tutorantId()
  url = a.api_link() + "student/" + tut
  #payload = "{\"studentID\": \""+ tut + "\",\n      \"firstName\": \"TestTutorant\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a student\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n}"  
  payload = "{\n\"studentID\": "+ a.tutorantId() +" ,\n      \"firstName\": \"TestCoach+1\",\n      \"surName\": \"Test\",\n      \"phoneNumber\": \"0692495724\",\n      \"interests\": \"Programming (C only), Servers, Cisco\",\n      \"photo\": \"https://i.imgur.com/Tl5sYD6.jpg\",\n      \"description\": \"I am a coach\",\n      \"degree\": \"HBO\",\n      \"study\": \"Technische Informatica\",\n      \"studyYear\": 4\n}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.put(url, data=payload, headers=headers)
  assert r.status_code == 204, r.status_code
def test_get_student():
  r = requests.get(a.api_link() + "students/search")
  assert r.status_code == 200, r.status_code
  assert len(r.json()) > 1, len(r.json())
  v = Validator(a.s_studentData())
  assert v.validate(r.json()[0]) == True, v.errors
