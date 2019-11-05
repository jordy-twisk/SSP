import pytest
import requests
import os
import api_info as a
from cerberus import Validator

def test_get_coaches():
  r = requests.get(a.api_link() + "profile/coach")
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_coach())
  assert v.validate(r.json()[0]['coach']) == True, v.errors
  v = Validator(a.s_studentData())
  assert v.validate(r.json()[0]['user']) == True, v.errors
def test_post_coach():
  a.delete_multiple_coachTutorant
  a.delete_multiple_tutorant
  a.delete_coach()
  url = a.api_link() + "profile/coach"
  payload = "{\n\"coach\": {\n      \"studentID\": \""+ a.coachId() + "\",\n      \"workload\": 5\n    },\n \"user\": {\n      \"studentID\": \""+ a.coachId() + "\",\n      \"firstName\": \"TestCoach\",\n      \"surName\": \"Test\"}}"  
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.post(url, data=payload, headers=headers)
  if r.status_code is not 201:
    a.create_coach()
    a.create_multiple_tutorant
    a.create_multiple_coachTutorant
  assert r.status_code == 201, r.status_code
  a.create_multiple_tutorant
  a.create_multiple_coachTutorant
def test_get_coachProfile_byId():
  r = requests.get(a.api_link() + "profile/coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_coach())
  assert v.validate(r.json()['coach']) == True, v.errors
  v = Validator(a.s_studentData())
  assert v.validate(r.json()['user']) == True, v.errors
def test_delete_coachProfile_byId():
  r = requests.delete(a.api_link() + "profile/coach/" + a.coachId())
  if r.status_code is not 204:
    a.create_coach()
  assert r.status_code == 204, r.status_code
  a.create_coach()
def test_get_coach_byId():
  r = requests.get(a.api_link() + "coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
  v = Validator(a.s_coach())
  assert v.validate(r.json()) == True, v.errors
def test_put_coach_byId():
  payload = "{\n\t\"studentID\": 000701,\n\t\"workload\": 5 \n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.put(a.api_link() + "coach/" + a.coachId(), data=payload, headers=headers)
  assert r.status_code == 204, r.text
