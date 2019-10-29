import pytest
import requests
import os
import api_info as a
from cerberus import Validator

def test_get_student_byId():
  r = requests.get(a.api_link() + "student/" + a.studentId())
  assert r.status_code == 200, r.status_code
  studentData = {	
    'studentID': 	{'type': 'integer'},
    'firstName': 	{'type': 'string'},
    'surName':	{'type': 'string'},
    'phoneNumber':	{'type': 'string'},
    'photo':	{'type': 'string'},
    'description':	{'type': 'string'},
    'degree':	{'type': 'string'},
    'study':	{'type': 'string'},
    'studyYear':	{'type': 'integer'},
    'interests':	{'type': 'string'}
  }
  v = Validator(studentData)
  assert v.validate(r.json()) == True, v.errors
def test_put_student_byId():
  r = requests.put(a.api_link() + "student/" + a.studentId())
  assert r.status_code == 200, r.status_code
def test_get_student():
  r = requests.get(a.api_link() + "students/search?studentId=" + a.studentId())
  assert r.status_code == 200, r.status_code
